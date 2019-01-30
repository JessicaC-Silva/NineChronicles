using System;
using System.Collections;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Libplanet;
using Libplanet.Crypto;
using Nekoyume.Data.Table;
using Nekoyume.Model;
using UnityEngine;

namespace Nekoyume.Action
{
    [Serializable]
    internal class SaveData
    {
        public Model.Avatar Avatar;
    }

    public class ActionManager : MonoBehaviour
    {
        private string _saveFilePath;

        private Agent agent;
        public BattleLog battleLog;
        public static ActionManager Instance { get; private set; }
        public Model.Avatar Avatar { get; private set; }
        public event EventHandler<Model.Avatar> DidAvatarLoaded;
        public event EventHandler CreateAvatarRequired;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        private void ReceiveAction(object sender, Context ctx)
        {
            var avatar = Avatar;
            Avatar = ctx.avatar;
            SaveStatus();
            if (avatar == null)
            {
                DidAvatarLoaded?.Invoke(this, Avatar);
            }
            battleLog = ctx.battleLog;
        }

        public void StartSync()
        {
            if (Avatar != null)
            {
                DidAvatarLoaded?.Invoke(this, Avatar);
            }

            var address = agent.UserAddress;
            var states = agent.blocks.GetStates(new[] {address});
            var ctx = (Context) states.GetValueOrDefault(address);
            if (ctx?.avatar == null)
            {
                CreateAvatarRequired?.Invoke(this, null);
            }

            StartCoroutine(Sync());
        }

        private IEnumerator Sync()
        {
            return agent.Sync();
        }

        public void CreateNovice(string nickName)
        {
            var action = new CreateNovice();
            ProcessAction(action);
        }

        private void LoadStatus()
        {
            if (!File.Exists(_saveFilePath)) return;

            var formatter = new BinaryFormatter();
            using (FileStream stream = File.Open(_saveFilePath, FileMode.Open))
            {
                var data = (SaveData) formatter.Deserialize(stream);
                Avatar = data.Avatar;
            }
        }

        private void SaveStatus()
        {
            var data = new SaveData
            {
                Avatar = Avatar,
            };
            var formatter = new BinaryFormatter();
            using (FileStream stream = File.Open(_saveFilePath, FileMode.OpenOrCreate))
            {
                formatter.Serialize(stream, data);
            }
        }

        private void StartMine()
        {
            StartCoroutine(agent.Mine());
        }

        public void UpdateItems(string serializeItems)
        {
            Avatar.Items = serializeItems;
            SaveStatus();
        }

        public void MoveStage(int stage)
        {
            var action = new MoveStage {stage = stage};
            ProcessAction(action);
        }

        public void Sleep(Stats statsData)
        {
            var action = new Sleep();
            ProcessAction(action);
        }

        private void ProcessAction(ActionBase action)
        {
            agent.queuedActions.Enqueue(action);
        }

        public void HackAndSlash()
        {
            var action = new HackAndSlash();
            ProcessAction(action);
        }

        public void Init(int index)
        {
            PrivateKey privateKey = null;
            var privateKeyHex = PlayerPrefs.GetString($"private_key_{index}", "");

            if (string.IsNullOrEmpty(privateKeyHex))
            {
                privateKey = new PrivateKey();
                PlayerPrefs.SetString("private_key", ByteUtil.Hex(privateKey.ByteArray));
            }
            else
            {
                privateKey = new PrivateKey(ByteUtil.ParseHex(privateKeyHex));
            }

            _saveFilePath = Path.Combine(Application.persistentDataPath, $"avatar_{index}.dat");
            LoadStatus();

            var path = Path.Combine(Application.persistentDataPath, "planetarium");
            agent = new Agent(privateKey, path);
            agent.DidReceiveAction += ReceiveAction;

            Debug.Log($"User Address: 0x{agent.UserAddress.ToHex()}");
            StartMine();
        }

        public void Combination()
        {
            var action = new Combination
            {
                material_1 = 101001,
                material_2 = 101001,
                material_3 = 101001,
                result = 301001,
            };
            ProcessAction(action);
        }
    }
}
