using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;

public class TimelineTrigger : MonoBehaviour
{
    public PlayableDirector timeline;

    [Header("Scripts à activer à la fin de la Timeline")]
    public List<MonoBehaviour> scriptsToEnable = new List<MonoBehaviour>();

    void Start()
    {
        if (timeline != null)
        {
            foreach (var script in scriptsToEnable)
            {
                if (script != null)
                    script.enabled = false;
            }

            timeline.stopped += OnTimelineStopped;
        }
    }

    void OnTimelineStopped(PlayableDirector obj)
    {
        foreach (var script in scriptsToEnable)
        {
            if (script != null)
            {
                script.enabled = true;
                Debug.Log($"Timeline terminée → {script.GetType().Name} activé !");
            }
        }
    }
}