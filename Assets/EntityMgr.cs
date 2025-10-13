using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMgr : MonoBehaviour
{
    public static EntityMgr inst;
    private void Awake()
    {
        inst = this;
        entities = new List<Entity>();
        foreach (Entity ent in movableEntitiesRoot.GetComponentsInChildren<Entity>())
        {
            entities.Add(ent);
        }
    }

    private void Start()
    {
        GameMgr.OnGameStarted += this.OnGameStarted;
        GameMgr.OnGameStopped += () => this.DestroyAllEntities();
    }

    private void OnGameStarted()
    {
        int numEntities = GameMgr.Mode switch
        {
            Mode.WayPointGeneration => 1,
            Mode.SingleEntityWaypointFollow => 5,
            Mode.GroupMovementPotentialField => 10,
            Mode.AStarPotentialField => 10,
            _ => 0
        };

        Vector3 startPos = GameMgr.Environment switch
        {
            Environment.AStar => new Vector3(150, 0, 162),
            Environment.Office => new Vector3(10, 0, 175),
            _ => Vector3.zero
        };

        for (int i = 0; i < numEntities; i++)
        {
            Vector3 newStart = startPos + new Vector3(5 * i, 0, 5 * i);
            CreateEntity(EntityType.TugBoat, newStart, Vector3.zero);
        }
    }

    public GameObject movableEntitiesRoot;
    public List<GameObject> entityPrefabs;
    public GameObject entitiesRoot;
    public List<Entity> entities;

    public static int entityId = 0;

    public Entity CreateEntity(EntityType et, Vector3 position, Vector3 eulerAngles)
    {
        Entity entity = null;
        GameObject entityPrefab = entityPrefabs.Find(x => x.GetComponent<Entity>().entityType == et);
        if (entityPrefab != null)
        {
            GameObject entityGo = Instantiate(entityPrefab, position, Quaternion.Euler(eulerAngles), entitiesRoot.transform);
            if (entityGo != null)
            {
                entity = entityGo.GetComponent<Entity>();
                entity.isSelected = false;
                entityGo.name = et.ToString() + entityId++;
                entities.Add(entity);
            }
        }
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        if (this.entities.Remove(entity))
            Destroy(entity.gameObject);
    }

    public void DestroyAllEntities()
    {
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            entities.RemoveAt(i);
            Destroy(entity.gameObject);
        }
    }
}
