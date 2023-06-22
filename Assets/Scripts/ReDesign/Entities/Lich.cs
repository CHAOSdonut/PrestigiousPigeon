using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReDesign.Entities
{
    public class Lich : Enemy
    {
        public override int SightRange { get { return 6; } }
        public override int MoveRange { get { return 2; } }

        public override string displayName{ get { return "Lich"; } }

        public Lich()
        {
            int MaxHealth = 8;
            _entityHealth = new UnitHealth(MaxHealth, MaxHealth);
            Attacks = new List<AttacksAndSpells>()
            {
                new LichAttack()
            };
        }

        public override void NextAction()
        {
            //Debug.Log("im a Lich");
            StateController.ChangeState(GameState.EnemyTurn);

            //Move() will call Attack() and change turn
            Move();
            
        }

        public override void Move()
        {
            DefaultTile currentTile = WorldController.ObstacleLayer.Where(o => o.GameObject == this.gameObject).FirstOrDefault();
            DefaultTile enemyPos = WorldController.getPlayerTile();
            int range = Math.Abs(currentTile.XPos - enemyPos.XPos) + Math.Abs(currentTile.YPos - enemyPos.YPos);
            if (range < SightRange)
            {
                MoveToPlayer(this.MoveRange);
            } else
            {
                MoveToPlayer(0);
            }
            
            //foreach(AttacksAndSpells atk in _attacks)
        }

        public override void Attack(AudioClip attackSound)
        {
            DefaultTile currentTile = WorldController.ObstacleLayer.Where(o => o.GameObject == this.gameObject).FirstOrDefault();
            List<DefaultTile> targetTiles = Attacks[0].GetTargetLocations(currentTile.XPos, currentTile.YPos);
            DefaultTile targetTile = targetTiles.Where(t => t.XPos == WorldController.getPlayerTile().XPos && t.YPos == WorldController.getPlayerTile().YPos).FirstOrDefault();
            if (targetTile != null)
            {
                StartCoroutine(EnemyRotateToAttack());
                SoundManager.Instance.PlaySound(attackSound);

                Attacks[0].Effect(targetTile.XPos, targetTile.YPos);
            }
            //attacking = false;
            StopCoroutine(EnemyRotateToAttack());
        }

        // public override void ReceiveDamage(int dmg)
        // {
        //     throw new NotImplementedException();
        // }
    }
}