using System;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;

namespace Nekoyume.UI.Module
{
    public class SkillView : VanillaSkillView
    {
        public RectTransform informationArea;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI powerText;
        public TextMeshProUGUI chanceText;
        
        private readonly List<IDisposable> _disposablesForModel = new List<IDisposable>();

        public Model.SkillView Model { get; private set; }

        public void SetData(Model.SkillView model)
        {
            if (model is null)
            {
                Hide();
                
                return;
            }

            _disposablesForModel.DisposeAllAndClear();
            Model = model;
            Model.name.SubscribeToText(nameText).AddTo(_disposablesForModel);
            Model.power.SubscribeToText(powerText).AddTo(_disposablesForModel);
            Model.chance.SubscribeToText(chanceText).AddTo(_disposablesForModel);

            base.SetData(model.iconSprite.Value);
        }

        public override void Hide()
        {
            base.Hide();
            Model?.Dispose();
            Model = null;
            _disposablesForModel.DisposeAllAndClear();
        }
    }
}