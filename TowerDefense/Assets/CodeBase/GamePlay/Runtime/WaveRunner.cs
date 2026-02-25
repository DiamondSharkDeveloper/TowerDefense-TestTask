using System;
using System.Collections;
using CodeBase.Enums;
using CodeBase.StaticData;
using UnityEngine;

public class WaveRunner : MonoBehaviour
{
    private LevelStaticData _levelData;
    private Action<CreatureTypeId> _spawn;
    private Action _onCompleted;

    public void Init(LevelStaticData levelData, Action<CreatureTypeId> spawn, Action onCompleted)
    {
        _levelData = levelData;
        _spawn = spawn;
        _onCompleted = onCompleted;
    }

    public void StartWaves()
    {
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        foreach (EnemyWaveData wave in _levelData.EnemyWaves)
        {
            if (wave.AppearanceTime > 0)
                yield return new WaitForSeconds(wave.AppearanceTime);

            foreach (CreatureOnWaveData creature in wave.CreatureOnWaveData)
            {
                for (int i = 0; i < creature.CreatureCount; i++)
                    _spawn?.Invoke(creature._typeId);
            }
        }

        _onCompleted?.Invoke();
    }
}