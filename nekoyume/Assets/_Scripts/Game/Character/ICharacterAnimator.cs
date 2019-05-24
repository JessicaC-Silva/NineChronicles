using System;
using UniRx;
using UnityEngine;

namespace Nekoyume.Game.Character
{
    public interface ICharacterAnimator : IDisposable
    {
        /// <summary>
        /// 캐릭터의 루트 게임 오브젝트.
        /// </summary>
        CharacterBase Root { get; }
        /// <summary>
        /// 컨트롤 하려는 애니메이터가 붙어 있는 게임 오브젝트.
        /// </summary>
        GameObject Target { get; }
        Subject<string> OnEvent { get; }
        float TimeScale { get; set; }

        void ResetTarget(GameObject value);
        void DestroyTarget();
        bool AnimatorValidation();
        Vector3 GetHUDPosition();
        
        #region Animation

        void Appear();
        void Idle();
        void Run();
        void StopRun();
        void Attack();
        void Cast();
        void Hit();
        void Die();
        void Disappear();

        #endregion
    }
}
