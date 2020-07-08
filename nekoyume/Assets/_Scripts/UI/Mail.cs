using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.SimpleLocalization;
using Nekoyume.Action;
using Nekoyume.Helper;
using Nekoyume.Model.Mail;
using Nekoyume.State;
using Nekoyume.UI.Model;
using Nekoyume.UI.Scroller;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class Mail : XTweenWidget, IMail
    {
        public enum MailTabState : int
        {
            All,
            Workshop,
            Auction,
            System
        }

        [Serializable]
        public class TabButton
        {
            private static readonly Vector2 LeftBottom = new Vector2(-15f, -10.5f);
            private static readonly Vector2 MinusRightTop = new Vector2(15f, 13f);

            public Sprite highlightedSprite;
            public Button button;
            public Image hasNotificationImage;
            public Image image;
            public Image icon;
            public TextMeshProUGUI text;
            public TextMeshProUGUI textSelected;

            public void Init(string localizationKey)
            {
                if (!button) return;
                var localized = LocalizationManager.Localize(localizationKey);
                var content = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(localized.ToLower());
                text.text = content;
                textSelected.text = content;
            }

            public void ChangeColor(bool isHighlighted = false)
            {
                image.overrideSprite = isHighlighted ? _selectedButtonSprite : null;
                var imageRectTransform = image.rectTransform;
                imageRectTransform.offsetMin = isHighlighted ? LeftBottom : Vector2.zero;
                imageRectTransform.offsetMax = isHighlighted ? MinusRightTop : Vector2.zero;
                icon.overrideSprite = isHighlighted ? highlightedSprite : null;
                text.gameObject.SetActive(!isHighlighted);
                textSelected.gameObject.SetActive(isHighlighted);
            }
        }

        // FIXME: Notification에서 mailIcons에 대한 의존이 있는데, 이것의 초기화가 Mail의 Initialize()에서 이루어지고 있어서 문제가 됩니다.
        // mailIcons의 내용으로 보아 리소스 캐싱으로 보이는데, Resources.Load<T>()는 내부에서 일정 용량까지 캐싱을 하고 있기 때문에 별도로 캐싱을 구현하지 않아도 됩니다.
        public static readonly Dictionary<MailType, Sprite> mailIcons =
            new Dictionary<MailType, Sprite>();

        [SerializeField]
        private MailTabState tabState = default;

        [SerializeField]
        private MailScroll scroll = null;

        [SerializeField]
        private TabButton[] tabButtons = null;

        [SerializeField]
        private GameObject emptyImage = null;

        [SerializeField]
        private TextMeshProUGUI emptyText = null;

        [SerializeField]
        private string emptyTextL10nKey = null;

        [SerializeField]
        private Blur blur = null;

        private static Sprite _selectedButtonSprite;

        public MailBox MailBox { get; private set; }

        #region override

        public override void Initialize()
        {
            base.Initialize();
            _selectedButtonSprite = Resources.Load<Sprite>("UI/Textures/button_yellow_02");

            var path = "UI/Textures/icon_mail_Auction";
            mailIcons.Add(MailType.Auction, Resources.Load<Sprite>(path));
            path = "UI/Textures/icon_mail_Workshop";
            mailIcons.Add(MailType.Workshop, Resources.Load<Sprite>(path));
            path = "UI/Textures/icon_mail_System";
            mailIcons.Add(MailType.System, Resources.Load<Sprite>(path));

            tabButtons[0].Init("ALL");
            tabButtons[1].Init("UI_COMBINATION");
            tabButtons[2].Init("UI_SHOP");
            tabButtons[3].Init("SYSTEM");
            ReactiveAvatarState.MailBox?.Subscribe(SetList).AddTo(gameObject);

            emptyText.text = LocalizationManager.Localize(emptyTextL10nKey);
        }

        public override void Show(bool ignoreShowAnimation = false)
        {
            tabState = MailTabState.All;
            MailBox = States.Instance.CurrentAvatarState.mailBox;
            ChangeState(0);
            UpdateTabs();
            base.Show(ignoreShowAnimation);

            if (blur)
            {
                blur.Show();
            }
        }

        public override void Close(bool ignoreCloseAnimation = false)
        {
            if (blur)
            {
                blur.Close();
            }

            base.Close(ignoreCloseAnimation);
        }

        #endregion

        public void UpdateTabs()
        {
            var blockIndex = Game.Game.instance.Agent.BlockIndex;
            // 전체 탭
            tabButtons[0].hasNotificationImage.enabled = MailBox
                .Any(mail => mail.New && mail.requiredBlockIndex <= blockIndex);

            for (var i = 1; i < tabButtons.Length; ++i)
            {
                tabButtons[i].hasNotificationImage.enabled = MailBox
                    .Any(mail =>
                        mail.MailType == (MailType) i && mail.New &&
                        mail.requiredBlockIndex <= blockIndex);
            }
        }

        public void ChangeState(int state)
        {
            tabState = (MailTabState) state;

            for (var i = 0; i < tabButtons.Length; ++i)
            {
                tabButtons[i].ChangeColor(i == state);
            }

            var list = MailBox
                .Where(i => i.requiredBlockIndex <= Game.Game.instance.Agent.BlockIndex).ToList();
            if (state > 0)
            {
                list = list.FindAll(mail => mail.MailType == (MailType) state);
            }

            scroll.UpdateData(list);
            emptyImage.SetActive(list.Count == 0);
        }

        private void SetList(MailBox mailBox)
        {
            if (mailBox is null)
            {
                return;
            }

            MailBox = mailBox;
            ChangeState((int) tabState);
        }

        public void Read(CombinationMail mail)
        {
            var avatarAddress = States.Instance.CurrentAvatarState.address;
            var attachment = (CombinationConsumable.ResultModel) mail.attachment;
            var item = attachment.itemUsable;
            var popup = Find<CombinationResultPopup>();
            var materialItems = attachment.materials
                .Select(pair => new {pair, item = pair.Key})
                .Select(t => new CombinationMaterial(
                    t.item,
                    t.pair.Value,
                    t.pair.Value,
                    t.pair.Value))
                .ToList();
            var model = new UI.Model.CombinationResultPopup(new CountableItem(item, 1))
            {
                isSuccess = true,
                materialItems = materialItems
            };
            model.OnClickSubmit.Subscribe(_ =>
            {
                LocalStateModifier.AddItem(avatarAddress, item.ItemId, false);
                LocalStateModifier.RemoveNewAttachmentMail(avatarAddress, mail.id, false);
                LocalStateModifier.RemoveAttachmentResult(avatarAddress, mail.id);
                LocalStateModifier.ModifyAvatarItemRequiredIndex(
                    avatarAddress,
                    item.ItemId,
                    Game.Game.instance.Agent.BlockIndex);
            });
            popup.Pop(model);
        }

        public void Read(SellCancelMail mail)
        {
            var avatarAddress = States.Instance.CurrentAvatarState.address;
            var attachment = (SellCancellation.Result) mail.attachment;
            var item = attachment.itemUsable;
            //TODO 관련 기획이 끝나면 별도 UI를 생성
            var popup = Find<ItemCountAndPricePopup>();
            var model = new UI.Model.ItemCountAndPricePopup();
            model.TitleText.Value = LocalizationManager.Localize("UI_RETRIEVE");
            model.InfoText.Value = LocalizationManager.Localize("UI_SELL_CANCEL_INFO");
            model.PriceInteractable.Value = false;
            model.Price.Value = attachment.shopItem.Price;
            model.CountEnabled.Value = false;
            model.Item.Value = new CountEditableItem(item, 1, 1, 1);
            model.OnClickSubmit.Subscribe(_ =>
            {
                LocalStateModifier.AddItem(avatarAddress, item.ItemId, false);
                LocalStateModifier.RemoveNewAttachmentMail(avatarAddress, mail.id);
                popup.Close();
            }).AddTo(gameObject);
            model.OnClickCancel.Subscribe(_ =>
            {
                //TODO 재판매 처리추가되야함\
                LocalStateModifier.AddItem(avatarAddress, item.ItemId, false);
                LocalStateModifier.RemoveNewAttachmentMail(avatarAddress, mail.id);
                popup.Close();
            }).AddTo(gameObject);
            popup.Pop(model);
        }

        public void Read(BuyerMail buyerMail)
        {
            var avatarAddress = States.Instance.CurrentAvatarState.address;
            var attachment = (Buy.BuyerResult) buyerMail.attachment;
            var item = attachment.itemUsable;
            var popup = Find<CombinationResultPopup>();
            var model = new UI.Model.CombinationResultPopup(new CountableItem(item, 1))
            {
                isSuccess = true,
                materialItems = new List<CombinationMaterial>()
            };
            model.OnClickSubmit.Subscribe(_ =>
            {
                LocalStateModifier.AddItem(avatarAddress, item.ItemId, false);
                LocalStateModifier.RemoveNewAttachmentMail(avatarAddress, buyerMail.id);
            }).AddTo(gameObject);
            popup.Pop(model);
        }

        public void Read(SellerMail sellerMail)
        {
            var agentAddress = States.Instance.AgentState.address;
            var avatarAddress = States.Instance.CurrentAvatarState.address;
            var attachment = (Buy.SellerResult) sellerMail.attachment;

            //TODO 관련 기획이 끝나면 별도 UI를 생성
            LocalStateModifier.ModifyAgentGold(agentAddress, attachment.gold);
            LocalStateModifier.RemoveNewAttachmentMail(avatarAddress, sellerMail.id);
        }

        public void Read(ItemEnhanceMail itemEnhanceMail)
        {
            var avatarAddress = States.Instance.CurrentAvatarState.address;
            var attachment = (ItemEnhancement.ResultModel) itemEnhanceMail.attachment;
            var popup = Find<CombinationResultPopup>();
            var item = attachment.itemUsable;
            var model = new UI.Model.CombinationResultPopup(new CountableItem(item, 1))
            {
                isSuccess = true,
                materialItems = new List<CombinationMaterial>()
            };
            model.OnClickSubmit.Subscribe(_ =>
            {
                LocalStateModifier.AddItem(avatarAddress, item.ItemId, false);
                LocalStateModifier.RemoveNewAttachmentMail(avatarAddress, itemEnhanceMail.id);
            });
            popup.Pop(model);
        }
    }
}
