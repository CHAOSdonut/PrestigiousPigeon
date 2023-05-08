﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReDesign.Entities
{
    public abstract class Entity : MonoBehaviour
    {
        public UnitHealth _entityHealth { get; set; }
        [SerializeField] private GameObject _healthBar;
        public List<AttacksAndSpells> Attacks { get; set; }
        public bool finishedMoving = false;
        public bool attacking = false;
        public IEnumerator movingCoroutine;
        private static GameObject _gameOver;
        private Vector3 targetLoc;
        public abstract void NextAction();
        public abstract void Move();
        public abstract void Attack();

        public virtual void ReceiveDamage(int dmg)
        {
            _entityHealth.ChangeHealth(-dmg);
            _healthBar.transform.localScale = (new Vector3(
                _entityHealth.HealthPercentage(_entityHealth.Health),
                (float)0.1584, (float)0.09899999));
            
            if (_entityHealth.Health <= 0)
            {
                if (this.gameObject.name.Contains("Player"))
                {
                    TurnController.gameOver = true;
                    PlayerAnimator._animator.SetBool("PlayerDead", true);
                }
                else
                {
                    //Add animation so it isnt instant
                    DefaultTile obstacleTile = WorldController.ObstacleLayer
                        .FirstOrDefault(t => t.GameObject == gameObject);
                    WorldController.Instance.BaseLayer.FirstOrDefault(t => t.XPos == obstacleTile.XPos && t.YPos == obstacleTile.YPos)
                        .Walkable = true;
                    WorldController.ObstacleLayer.RemoveAt(WorldController.ObstacleLayer.IndexOf(obstacleTile));
                    obstacleTile.GameObject = null;
                    obstacleTile = null;
                    Destroy(this.gameObject);
                }

                TurnController.Instance.gameOverEvent.Invoke();
            }
        }

        public void MoveToPlayer(int movementRange)
        {
            DefaultTile currentTile = WorldController.ObstacleLayer
                .FirstOrDefault(o => o.GameObject == this.gameObject);
            List<DefaultTile> targetLocations = Attacks[0].GetTargetLocations(currentTile.XPos, currentTile.YPos);
            DefaultTile enemyPos = WorldController.getPlayerTile();
            if (targetLocations.FirstOrDefault(t => t.XPos == enemyPos.XPos && t.YPos == enemyPos.YPos) == null)
            {
                int widthAndHeight = (int)Mathf.Sqrt(WorldController.Instance.BaseLayer.Count);
                PlayerPathfinding pf =
                    new PlayerPathfinding(widthAndHeight, widthAndHeight, WorldController.Instance.BaseLayer);

                List<DefaultTile> path = null;

                foreach (DefaultTile dt in pf.GetNeighbourList(enemyPos))
                {
                    List<DefaultTile> newPath = pf.FindPath(currentTile.XPos, currentTile.YPos, dt.XPos, dt.YPos);
                    if (newPath != null && (path == null || newPath.Count < path.Count))
                    {
                        path = newPath;
                    }
                }


                Debug.Log("PlayerPos = " + enemyPos.XPos);
                if (path != null)
                {
                    List<DefaultTile> actualPath = new List<DefaultTile>();
                    actualPath.AddRange(path.GetRange(0, movementRange + 1));
                    actualPath.First().Walkable = true;
                    actualPath.Last().Walkable = false;


                    movingCoroutine = EntityMoveSquares(actualPath);
                    StartCoroutine(movingCoroutine);
                    currentTile.XPos = actualPath.Last().XPos;
                    currentTile.YPos = actualPath.Last().YPos;
                }
                else
                {
                    finishedMoving = true;
                }
            }
            else
            {
                finishedMoving = true;
                attacking = true;
            }
        }
        
        public IEnumerator EntityMoveSquares(List<DefaultTile> path)
        {
            GridLayout gr = WorldController.Instance.gridLayout;
            // For loop to loop over the path
            for (int i = 1; i < path.Count; i++)
            {
                DefaultTile pathNode = path[i];
                Vector3 targetPos = new Vector3(pathNode.GameObject.transform.position.x, transform.position.y, pathNode.GameObject.transform.position.z);
                Vector3 dir = (targetPos - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(dir,Vector3.up);
                targetLoc = SnapCoordinateToGrid(targetPos, gr);
                float time = 0;
                while (time < 0.5f)
                {
                    // Adds the position and rotation
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, time / 0.5f);
                    transform.rotation = targetRotation;
                    transform.position = Vector3.MoveTowards(transform.position, targetLoc, Time.deltaTime * 5);
                    time += Time.deltaTime;
                    yield return null;
                }
            }

            finishedMoving = true;
        }

        private Vector3 SnapCoordinateToGrid(Vector3 position, GridLayout gridLayout)
        {
            Vector3Int cellPos = gridLayout.WorldToCell(position);
            Grid grid = gridLayout.gameObject.GetComponent<Grid>();
            position = new Vector3(grid.GetCellCenterWorld(cellPos).x, position.y, grid.GetCellCenterWorld(cellPos).z);
            // Change Y position of player to match grid here
            return position;
        }

        public virtual void Update()
        {
            if (finishedMoving == true)
            {
                attacking = true;
                //finishedMoving = false;
            }

            if (attacking)
            {
                Attack();
            }

            if (finishedMoving && !attacking)
            {
                finishedMoving = false;
                StateController.ChangeState(GameState.EndTurn);
            }
        }
    }
}