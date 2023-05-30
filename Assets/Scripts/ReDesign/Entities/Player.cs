﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReDesign.Entities
{
    public class Player : Entity
    {
        private static Transform player;
        private static Vector3 targetLocation;
        [SerializeField] private ManaSystem _manaSystem;
        
        public override int SightRange { get; }
        public override int MoveRange { get { return _manaSystem.GetMana(); } }
        
        private List<AttacksAndSpells> _attacks = new List<AttacksAndSpells>
        {
            new BasicFireSpell(),
            new BasicIceSpell()
        };

        public virtual void Awake()
        {
            int MaxHealth = 20;
            _entityHealth = new UnitHealth(MaxHealth, MaxHealth);
            player = transform;
        }

        private void Start()
        {
            RangeTileTool.Instance.drawMoveRange(WorldController.getPlayerTile(), _manaSystem.GetMana());
        }

        public override void Update()
        {
            //Player needs to override update since the entity update will end the players turn at the en of movement.
        }

        public override void NextAction()
        {
            StateController.ChangeState(GameState.PlayerTurn);
            Debug.Log("im a player");
            //StateController.ChangeState(GameState.EndTurn);
            _manaSystem.StartTurn();
            RangeTileTool.Instance.drawMoveRange(WorldController.getPlayerTile(), _manaSystem.GetMana());
        }

            
        public override void Move()
        {
            //throw new System.NotImplementedException();
        }

        public override void Attack()
        {

        }

        public void EndTurn() => StateController.ChangeState(GameState.EndTurn);

        public override void ReceiveDamage(int dmg)
        {
            base.ReceiveDamage(dmg);
            PlayerAnimator._animator.SetBool("isHit", true);
            PlayerAnimator._animator.SetBool("isIdle", false);
        }

        public static IEnumerator RotateToAttack()
        {
            Vector3 attackerPos = player.transform.position;
            Vector3 targetPos = MouseController.GetMouseWorldPos();
            GridLayout gr = WorldController.Instance.gridLayout;
            // Calculate the direction to the target position and set the entity's rotation accordingly
            Vector3 targetPosition = new Vector3(targetPos.x, attackerPos.y, targetPos.z);
            Vector3 dir = (targetPosition - attackerPos).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
            targetLocation = PlayerMovement.SnapCoordinateToGrid(targetPos, gr);
            float time = 0;

            // Loop until the entity has moved halfway to the target location
            while (time < 0.5f)
            {
                // Adds the position and rotation
                player.transform.rotation = Quaternion.Lerp(player.transform.rotation, targetRotation, time / 0.5f);
                time += Time.deltaTime;
                yield return null;
            }
            player.transform.rotation = targetRotation;
        }
    }
}