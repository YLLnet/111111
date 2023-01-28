using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class TouchControl : MonoBehaviour,IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    [SerializeField]Text text;
    [SerializeField] GameObject turntable;
    [SerializeField] bool isRotate;
    Vector3 turntablePos;
    private Vector2 startPos = Vector2.zero;
    private Vector2 endPos = Vector2.zero;
    private bool isDown = false;//拖动时间


    [SerializeField]
    private float rotateSpeed = 0;//选择速度
    private float rotateDirection = 1;//选转方向

    [Header("速度衰减时间")]
    [SerializeField] float decayTimeMax = 0.1f;

    [Header("旋转时间条件")]
    [SerializeField] float timeMax = 1f;


    [Header("灵敏度")]
    [SerializeField] float quick = 0.3f;

    [Header("横向偏移率")]
    [SerializeField] float widthRate = 0.1f;
    [Header("纵向偏移率")]
    [SerializeField] float heightRate = 0.1f;

    [SerializeField] float lerpTime = 20f;

    MyTimer checkTimer;
    MyTimer decayTimer;
    MyTimer dragTimer;

    float r_Speed;
    float r_Z;
    void Start()
    {
        Application.targetFrameRate = 60;
        turntablePos = Camera.main.WorldToScreenPoint(turntable.transform.position);

        //计时器 检测是否有触摸操作
        checkTimer = Timer.Ins.AddTimer(10,false,false,false,()=> { Debug.Log("10秒没操作"); });

        //计时器 0.1秒刷新
        decayTimer = Timer.Ins.AddTimer(decayTimeMax,true, true, false, () => {

            if (!isRotate || rotateSpeed <= 1.5f) return;

            rotateSpeed = Mathf.Lerp(rotateSpeed, 0, 1f / lerpTime);
          
            if (rotateSpeed <= 1.5f)
            {
                rotateSpeed = 0;
                decayTimer.timeStart = 0;
                isRotate = false;
            }
        });
       
        dragTimer = Timer.Ins.AddTimer(timeMax,false, false, false, () => {
            Debug.Log("长按已超过1秒");
        });
        
    }

    void FixedUpdate()
    {
        //有屏幕手指 接触
        // if (Input.touchCount > 0) 
        // CheckTimer.ReStart();//更新触摸时间
        // 
        r_Speed = (r_Z - turntable.transform.eulerAngles.z) * 60 * 60 / 360;
        if (r_Z != turntable.transform.eulerAngles.z)
        {
            r_Z = turntable.transform.eulerAngles.z;
        }
        text.text = string.Format("{0}转/min",Mathf.Floor(Mathf.Abs(r_Speed)));
        TurntableRotate();
    }

    //旋转更新
    private void TurntableRotate()
    {
        if (!isRotate || rotateSpeed <= 1.5f) { isRotate = false; return; }
        turntable.transform.Rotate(Vector3.forward  * rotateDirection * rotateSpeed);     
    }

    //按下
    public void OnPointerDown(PointerEventData eventData)
    {
        startPos = eventData.position;
        
        dragTimer.ReStart();
    }


    private Vector3 m_CurrentPos;            //记录当前帧鼠标所在位置
    private bool m_IsClockwise;              //是否顺时针
    // 拖动中。。。
    public void OnDrag(PointerEventData eventData)
    {
       
        Vector2 pos;
       
        pos.x = eventData.position.x -turntablePos.x;
        pos.y = eventData.position.y - turntablePos.y;

        Vector3 pos3 = new Vector3(pos.x, pos.y, 0);   //计算拖动点 相对 转盘的位置

        if (!isDown)
        {
            m_CurrentPos = pos3;
            isDown = true;
        }
       
       
        Vector3 currentPos = Vector3.Cross(pos3, m_CurrentPos);      //计算当前帧和上一帧手指位置 用于判断旋转方向
        if (currentPos.z > 0)
        {
            m_IsClockwise = true;
        }
        else if (currentPos.z < 0)
        {
            m_IsClockwise = false;
        }
      
        if (m_CurrentPos != pos3)    
        {
            if (m_IsClockwise)
            {
                //m_RoundValue += Vector3.Angle(m_CurrentPos, pos3);
                turntable.transform.Rotate(new Vector3(0, 0, -Vector3.Angle(m_CurrentPos, pos3)));             
            }

            else
            {
               // m_RoundValue -= Vector3.Angle(m_CurrentPos, pos3);
                turntable.transform.Rotate(new Vector3(0, 0, Vector3.Angle(m_CurrentPos, pos3)));           
            }
        }
        m_CurrentPos = pos3;

        // text.text = string.Format("{0},{1}", Mathf.Floor(eventData.position.x), Mathf.Floor(eventData.position.y));
    }

    //抬起
    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;  
        if (!dragTimer.isUpdate)
        {      
            return;
        }
        dragTimer.Stop();

        float offX = endPos.x - startPos.x;
        float offY = endPos.y - startPos.y;

        //取最大的滑动距离,判断是否开始滑动
        float length = Mathf.Abs(offX) > Mathf.Abs(offY) ? offX : offY;
   
        float rate = 0;
        if (length == offX)
        {
            rate = Mathf.Abs(length) / Convert.ToSingle(Screen.width);
            if (rate > widthRate && !isRotate)
            {
                isRotate = true;
            }

        }
        else if (length == offY)
        {
            rate = Mathf.Abs(length) / Convert.ToSingle(Screen.height);
            if (rate > heightRate && !isRotate)
            {
                isRotate = true;
            }
        }




        //计算按下坐标和松手时的计算x和y的最大偏移
        endPos = eventData.position;


        Vector2 pos;
        pos.x = eventData.position.x - turntablePos.x;
        pos.y = eventData.position.y - turntablePos.y;

        Vector3 pos3 = new Vector3(pos.x, pos.y, 0);   //计算拖动点 相对 转盘的位置

        //求角速度
        float angleSpeed = Vector3.Angle(startPos - new Vector2(turntablePos.x, turntablePos.y), endPos - new Vector2(turntablePos.x, turntablePos.y));

        Vector3 currentPos = Vector3.Cross(startPos - new Vector2(turntablePos.x, turntablePos.y), endPos - new Vector2(turntablePos.x, turntablePos.y));

        if (currentPos.z < 0)
        {
            rotateDirection = -1;
           
        }
        else
        {
            rotateDirection = 1;
           
        }

        rotateSpeed += angleSpeed* rate*quick;

    }


}



