﻿using Nekoyume.Helper;
using UnityEngine;

namespace Nekoyume
{
    public static class GameConfig
    {
        public const int SlotCount = 3;
        public const float WaitSeconds = 180f;
        public const string AvatarNickNamePattern = @"^[0-9가-힣a-zA-Z]{2,20}$";
        public const float PlayerSpeechBreakTime = 2f;
        
        #region action

        public const int DefaultAvatarCharacterId = 100010;
        public const int DefaultAvatarWeaponId = 10100000;
        public const int DefaultAvatarArmorId = 10200000;
        public const int ActionPoint = 1000;
        
        public const int HackAndSlashCostAP = 1;

        public const int CombineConsumableCostAP = 5;
        public const int CombineEquipmentCostAP = 5;
        public const int CombineEquipmentCostNCG = 5;
        public const int EnhanceEquipmentCostAP = 5;
        public const decimal CombinationValueP1 = 3m; // 30f;
        public const decimal CombinationValueP2 = 1m; // 1.2f;
        public const decimal CombinationValueL1 = 10m;
        public const decimal CombinationValueL2 = 1m;
        public const decimal CombinationValueR1 = 1.5m; // 2f;
        public const int CombinationDefaultFoodId = 200000;
        
        #endregion

        #region Color
        
        public const string ColorHexForGrade1 = "fff9dd";
        public const string ColorHexForGrade2 = "12ff00";
        public const string ColorHexForGrade3 = "0f91ff";
        public const string ColorHexForGrade4 = "ffae00";
        public const string ColorHexForGrade5 = "f73e26";

        public static readonly Color ColorForGrade1 = ColorHelper.HexToColorRGB(ColorHexForGrade1);
        public static readonly Color ColorForGrade2 = ColorHelper.HexToColorRGB(ColorHexForGrade2);
        public static readonly Color ColorForGrade3 = ColorHelper.HexToColorRGB(ColorHexForGrade3);
        public static readonly Color ColorForGrade4 = ColorHelper.HexToColorRGB(ColorHexForGrade4);
        public static readonly Color ColorForGrade5 = ColorHelper.HexToColorRGB(ColorHexForGrade5);

        #endregion
    }
}
