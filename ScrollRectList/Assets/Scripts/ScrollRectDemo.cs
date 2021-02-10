using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectDemo : MonoBehaviour
{

    //生成物体的父物体
    public Transform RankGrid;
    //拖动组件
    public ScrollRect scorllRect;

    /// <summary>
    /// 所有数据的集合
    /// </summary>
    public List<int> datas = new List<int> { 1, 3, 5, 7, 9, 10, 11, 12, 13, 14, 15 };

    Dictionary<GameObject, int> datasAndIndex = new Dictionary<GameObject, int>();

    private bool isDragIng = false;
    private bool isLoadingRecord = false;

    void Start()
    {
        //监听拖动
        scorllRect.onValueChanged.AddListener((value) => { 
            OnRecordDrag(value.y); 
        });
        //设置生成记录
        SetRecords(datas);
    }


    public void SetRecords(List<int> infos)
    {
        ClearRecord();
        StartCoroutine(SetRecord(infos));
    }

    public IEnumerator SetRecord(List<int> infos)
    {
        while (isLoadingRecord)
        {
            yield return new WaitForFixedUpdate();
        }

        isLoadingRecord = true;
        datas.Clear();
        datas.AddRange(infos);

        int h = 0;
        foreach (var item in datas)
        {
            h += 90;
        }
        //设置要拖动的物体宽高
        RankGrid.GetComponent<RectTransform>().sizeDelta = new Vector3(1080, h);
        RankGrid.GetComponent<RectTransform>().localPosition = Vector3.zero;
        isDragIng = true;
        //第一次刷新生成物体
        for (int i = 0; i < (datas.Count > 6 ? 6 : datas.Count); i++)
        {
            GameObject go = Instantiate(Resources.Load("Info")) as GameObject;
            go.transform.SetParent(RankGrid);
            SetRecordItem(i, go);
            yield return new WaitForSeconds(0.1f);
        }
        isDragIng = false;
        isLoadingRecord = false;
    }

    //设置历史比赛记录数据
    void SetRecordItem(int index, GameObject go)
    {
        if (index >= datas.Count) return;
        datasAndIndex[go] = index;

        //设置位置
        go.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1);
        go.transform.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(540, GetPos_Y(index), 0);

        //设置信息显示
        go.transform.GetChild(0).GetComponent<Text>().text = "我是第" + index + "条数据信息";

    }

    //拖动监听方法
    void OnRecordDrag(float y)
    {
        if (datas.Count <= 8) return;
        if (isDragIng) return;
        isDragIng = true;
        int indexNow = GetIndex(RankGrid.GetComponent<RectTransform>().anchoredPosition3D.y);
        //Debug.Log("y: " + y + ".. 比赛历史记录  indexNow ... " + LSBSPanel_LSZJGrid.GetComponent<RectTransform>().anchoredPosition3D.y + "  DataCount ..." + datas.Count);
        List<GameObject> needDispose = new List<GameObject>();
        foreach (var go in datasAndIndex.Keys)
        {
            if (datasAndIndex[go] >= indexNow && datasAndIndex[go] < indexNow + 6)
            {
                //没超出范围 暂且留着
                continue;
            }
            else
            {
                //超出范围,收回到对象池内
                needDispose.Add(go);
            }
        }
        foreach (var go in needDispose)
        {
            datasAndIndex.Remove(go);
            Destroy(go); //正常应该回收到对象池
        }
        for (int i = indexNow; i < indexNow + 6; i++)
        {
            if (datasAndIndex.ContainsValue(i))
            {
                //此位置已经有item了 不做处理
                continue;
            }
            else//此位置没有item 需要加载一个
            {
                if (i < datas.Count)
                {
                    GameObject item = Instantiate(Resources.Load("Info")) as GameObject;
                    item.transform.SetParent(RankGrid);
                    SetRecordItem(i, item);
                }
            }
        }
        isDragIng = false;
    }

    float GetPos_Y(int index)
    {
        float sizeY = 0;
        for (int i = 0; i < index; i++)
        {
            sizeY += 90;
        }
        return -sizeY;
    }

    int GetIndex(float y)
    {
        int index = 0;
        float sizeY = 0;
        for (int i = 0; i < datas.Count; i++)
        {
            sizeY += 90;
            if (sizeY > y)
            {
                index = i;
                break;
            }
        }
        return index;
    }

    //清理记录
    public void ClearRecord()
    {
        datas = new List<int>();
        foreach (var go in datasAndIndex.Keys)
        {
            //回收到对象池
            go.SetActive(false);
        }
        datasAndIndex.Clear();
    }
}
