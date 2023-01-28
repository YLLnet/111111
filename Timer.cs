using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{

    List<MyTimer> myTimers = new List<MyTimer>();
    public static Timer Ins;
    void Awake()
    {
        Ins = this;
    }

    public MyTimer AddTimer(float te, bool isgoon, bool loop, bool remove, Action action)
    {
        MyTimer myTimer = new MyTimer( te, isgoon, loop, remove, action);
        myTimers.Add(myTimer);
        return myTimer;
    }

    void FixedUpdate()
    {
        if (myTimers.Count>0)
        {
            for (int i = myTimers.Count -1; i >=0; i--)
            {
                if (myTimers[i].isUpdate)
                {
                    myTimers[i].Update(Time.deltaTime);
                    if (myTimers[i].timeStart>= myTimers[i].timeEnd)
                    {
                        myTimers[i].timeStart = 0;
                        myTimers[i].ac.Invoke();
                        if (!myTimers[i].isLoop)
                        {
                            //Debug.Log("¼ÆÊ±½áÊø");
                            myTimers[i].Stop();
                        }
                        else
                        {
                            myTimers[i].ReStart();
                        }

                        if (myTimers[i].isRemove)
                        {
                            myTimers.RemoveAt(i);
                        }
                    }
                }
            }
        }


    }



}


public class MyTimer
{

    public float timeStart;
    public float timeEnd;
    public bool isLoop;
    public bool isRemove;
    public bool isUpdate;
    public Action ac;

    public MyTimer() { }

    public MyTimer( float te,bool isgoon,bool loop,bool remove,Action action)
    {
        timeStart = 0;
        timeEnd = te;
        isLoop = loop;
        isRemove = remove;
        isUpdate = isgoon;
        ac = action;
    }

    public void Update(float value)
    {
        timeStart += value;
    }

    public void ReStart()
    {
        timeStart = 0;
        isUpdate = true;
    }

    public void ReStart(float te)
    {
        timeStart = 0;
        timeEnd = te;
        isUpdate = true;
    }

    public void Pause()
    {
        isUpdate = false;
    }

    public void GoOn()
    {
        isUpdate = true;
    }

    public void Stop()
    {
        isUpdate = false;
    }

}