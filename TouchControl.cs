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
    private bool isDown = false;//�϶�ʱ��


    [SerializeField]
    private float rotateSpeed = 0;//ѡ���ٶ�
    private float rotateDirection = 1;//ѡת����

    [Header("�ٶ�˥��ʱ��")]
    [SerializeField] float decayTimeMax = 0.1f;

    [Header("��תʱ������")]
    [SerializeField] float timeMax = 1f;


    [Header("������")]
    [SerializeField] float quick = 0.3f;

    [Header("����ƫ����")]
    [SerializeField] float widthRate = 0.1f;
    [Header("����ƫ����")]
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

        //��ʱ�� ����Ƿ��д�������
        checkTimer = Timer.Ins.AddTimer(10,false,false,false,()=> { Debug.Log("10��û����"); });

        //��ʱ�� 0.1��ˢ��
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
            Debug.Log("�����ѳ���1��");
        });
        
    }

    void FixedUpdate()
    {
        //����Ļ��ָ �Ӵ�
        // if (Input.touchCount > 0) 
        // CheckTimer.ReStart();//���´���ʱ��
        // 
        r_Speed = (r_Z - turntable.transform.eulerAngles.z) * 60 * 60 / 360;
        if (r_Z != turntable.transform.eulerAngles.z)
        {
            r_Z = turntable.transform.eulerAngles.z;
        }
        text.text = string.Format("{0}ת/min",Mathf.Floor(Mathf.Abs(r_Speed)));
        TurntableRotate();
    }

    //��ת����
    private void TurntableRotate()
    {
        if (!isRotate || rotateSpeed <= 1.5f) { isRotate = false; return; }
        turntable.transform.Rotate(Vector3.forward  * rotateDirection * rotateSpeed);     
    }

    //����
    public void OnPointerDown(PointerEventData eventData)
    {
        startPos = eventData.position;
        
        dragTimer.ReStart();
    }


    private Vector3 m_CurrentPos;            //��¼��ǰ֡�������λ��
    private bool m_IsClockwise;              //�Ƿ�˳ʱ��
    // �϶��С�����
    public void OnDrag(PointerEventData eventData)
    {
       
        Vector2 pos;
       
        pos.x = eventData.position.x -turntablePos.x;
        pos.y = eventData.position.y - turntablePos.y;

        Vector3 pos3 = new Vector3(pos.x, pos.y, 0);   //�����϶��� ��� ת�̵�λ��

        if (!isDown)
        {
            m_CurrentPos = pos3;
            isDown = true;
        }
       
       
        Vector3 currentPos = Vector3.Cross(pos3, m_CurrentPos);      //���㵱ǰ֡����һ֡��ָλ�� �����ж���ת����
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

    //̧��
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

        //ȡ���Ļ�������,�ж��Ƿ�ʼ����
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




        //���㰴�����������ʱ�ļ���x��y�����ƫ��
        endPos = eventData.position;


        Vector2 pos;
        pos.x = eventData.position.x - turntablePos.x;
        pos.y = eventData.position.y - turntablePos.y;

        Vector3 pos3 = new Vector3(pos.x, pos.y, 0);   //�����϶��� ��� ת�̵�λ��

        //����ٶ�
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



