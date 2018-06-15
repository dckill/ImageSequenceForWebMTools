using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Text.RegularExpressions;

public class Manager : MonoBehaviour {

    public GameObject BtnItem; //列表元素
    public RectTransform FolderRoot, FileRoot, ClipRoot; //三个列表根节点
    public Toggle[] ToggleList; //转换器开关列表
    public InputField[] InputFieldList; //输入器列表
    public VideoPlayer vp; //视频播放器
    public GameObject RenamePanel; //改名面板
    public InputField[] RenameInputList; //改名输入器列表
    public Text textShow; //改名面板实例
    public GameObject Log;


    private string CurrentInputPath = "";
    private float UpdateTime = 0.0f;
    // Use this for initialization
    void Start()
    {
        //更新ReaderTexture
        RenderTexture rt = new RenderTexture(530, 290, 0, RenderTextureFormat.ARGB32);
        vp.GetComponent<RawImage>().texture = rt;
        vp.targetTexture = rt;
    }
	
	// Update is called once per frame
	void Update () {

        //目录更新计时
        UpdateTime += Time.deltaTime;
        if (UpdateTime > 1)
        {
            UpdateTime = 0;
            UpdateFolder();
        }


    }

    //private void OnApplicationFocus(bool focus)
    //{
    //    //更新目录
    //    UpdateFolder();
    //}

        //更新目录方法
    void UpdateFolder()
    {
        //初始输入输出目录
        string InputDir = Application.dataPath + "/../Input/";
        string OutputDir = Application.dataPath + "/../Output/";

        //判断目录是否存在并创建
        if (!Directory.Exists(InputDir))
            Directory.CreateDirectory(InputDir);
        if (!Directory.Exists(OutputDir))
            Directory.CreateDirectory(OutputDir);

        //删除目录列表元素
        Button[] BtnList = FolderRoot.GetComponentsInChildren<Button>();
        for (int i = 0; i < BtnList.Length; i++)
        {
            Destroy(BtnList[i].gameObject);
        }
        
        //创建目录列表元素
        DirectoryInfo dir = new DirectoryInfo(InputDir);
        DirectoryInfo[] dii = dir.GetDirectories();
        if (dii.Length > 0)
        {
            foreach (var item in dii)
            {
                GameObject btn = Instantiate(BtnItem, FolderRoot);
                btn.GetComponentInChildren<Text>().text = item.Name;

                //添加目录按钮点击事件
                btn.GetComponent<Button>().onClick.AddListener(delegate
                {
                    //删除图片列表
                    CurrentInputPath = item.ToString()+"/";
                    Button[] BtnList2 = FileRoot.GetComponentsInChildren<Button>();
                    for (int i = 0; i < BtnList2.Length; i++)
                    {
                        Destroy(BtnList2[i].gameObject);
                    }

                    //创建图片列表元素
                    var tmpDir = InputDir  + item.Name + "/";
                    DirectoryInfo dir2 = new DirectoryInfo(tmpDir);
                    var files = dir2.GetFiles();
                    if (files.Length > 0)
                    {
                        List<FileInfo> filesname = new List<FileInfo>();
                        foreach (var item2 in files)
                        {
                            if (item2.Extension == ".png" || item2.Extension == ".jpg")
                                filesname.Add(item2);
                        }
                        for (int i = 0; i < filesname.Count; i++)
                        {
                            GameObject fileBtn = Instantiate(BtnItem, FileRoot);
                            fileBtn.GetComponentInChildren<Text>().text = filesname[i].Name;
                            //Destroy(fileBtn.GetComponent<Button>());
                        }
                        
                    }
                });
            }
        }

        //删除视频列表元素
        Button[] BtnList3 = ClipRoot.GetComponentsInChildren<Button>();
        for (int i = 0; i < BtnList3.Length; i++)
        {
            Destroy(BtnList3[i].gameObject);
        }

        //创建视频列表元素
        DirectoryInfo dir3 = new DirectoryInfo(OutputDir);
        var files2 = dir3.GetFiles();
        List<FileInfo> filesname2 = new List<FileInfo>();
        foreach (var item in files2)
        {
            if (item.Extension == ".webm")
                filesname2.Add(item);
        }

        for (int i = 0; i < filesname2.Count; i++)
        {
            GameObject fileBtn = Instantiate(BtnItem, ClipRoot);
            fileBtn.GetComponentInChildren<Text>().text = filesname2[i].Name;

            //添加视频列表按钮点击事件
            fileBtn.GetComponent<Button>().onClick.AddListener(delegate 
            {
                vp.source = VideoSource.Url;
                vp.url = OutputDir + fileBtn.GetComponentInChildren<Text>().text;
                vp.Play();
            });
        }

    }

    //转换启动按钮
    public void ConverBtn()
    {
        if (CurrentInputPath == "")
            return;

        Regex cn = new Regex("[\u4e00-\u9fa5]+");//正则表达式 表示汉字范围 
        DirectoryInfo dir = new DirectoryInfo(CurrentInputPath);
        var files = dir.GetFiles();
        if (files.Length == 0)
            return;

        List<FileInfo> filesname = new List<FileInfo>();
        foreach (var item in files)
        {
            if (item.Extension == ".png"|| item.Extension == ".jpg")
                filesname.Add(item);
        }
        
        foreach (var item in filesname)
        {
            if (cn.IsMatch(item.Name))
            {
                //文件名中包含中文, 需要更名
                RenamePanel.gameObject.SetActive(true);
                Log.SetActive(true);
                Log.GetComponentInChildren<Text>().text = "文件名中包含中文, 需要更名!";
                return;
            }
        }

        
        string CommandLine = "";

        if (ToggleList[0].isOn)
            CommandLine += " -r " + InputFieldList[0].text;

        if (ToggleList[1].isOn)
            CommandLine += " -i " + CurrentInputPath + InputFieldList[1].text;

        var pathstr = InputFieldList[1].text;

        string subStr= pathstr.Substring(0, pathstr.IndexOf('%'));
        if (filesname[0].Name.Substring(0, subStr.Length) != subStr)
        {
            Debug.Log("参数与文件名前缀不相符!");
            Log.SetActive(true);
            Log.GetComponentInChildren<Text>().text = "参数与文件名前缀不相符!";
            return;
        }

        //print(CurrentInputPath + InputFieldList[1].text);

        if (ToggleList[2].isOn)
            CommandLine += " -auto-alt-ref 0 ";

        if (ToggleList[3].isOn)
            CommandLine += " -vcodec " + InputFieldList[3].text;

        if (ToggleList[4].isOn)
            CommandLine += " -b " + InputFieldList[4].text;

        if (ToggleList[5].isOn)
            CommandLine += " -y ";

        if (ToggleList[6].isOn)
            CommandLine += " -s " + InputFieldList[6].text;

        if (ToggleList[7].isOn)
        {
            if (InputFieldList[7].text.ToString().IndexOf(".webm") == -1)
            {
                CommandLine += " " + Application.dataPath + "/../Output/" + InputFieldList[7].text + ".webm";
            }
            else
            {
                CommandLine += " " + Application.dataPath + "/../Output/" + InputFieldList[7].text;
            }
        }

        //print(CommandLine);
        if (vp.isPlaying)
            vp.Stop();

        ExecuteProgram("ffmpeg", Application.streamingAssetsPath, CommandLine);

    }

    public void RenameBtn()
    {
        DirectoryInfo dir = new DirectoryInfo(CurrentInputPath);
        var files = dir.GetFiles();
        List<FileInfo> filesname = new List<FileInfo>();
        foreach (var item in files)
        {
            if (item.Extension == ".png"|| item.Extension == ".jpg")
                filesname.Add(item);
        }

        //foreach (var item in filesname)
        for (int i = 0; i < filesname.Count; i++)
        {
            string tmpstr = "";
            if (int.Parse(RenameInputList[2].text) == 1)
            {
                filesname[i].MoveTo(CurrentInputPath+RenameInputList[0].text + i + RenameInputList[1].text+ filesname[i].Extension);
            }
            else if (int.Parse(RenameInputList[2].text) == 2)
            {
                if (i < 10)
                    tmpstr = "0";
                else
                    tmpstr = "";

                filesname[i].MoveTo(CurrentInputPath + RenameInputList[0].text + tmpstr + i + RenameInputList[1].text + filesname[i].Extension);
            }
            else if (int.Parse(RenameInputList[2].text) == 3)
            {
                if (i < 10)
                    tmpstr = "00";
                else if (i>9&&i<100)
                    tmpstr = "0";
                else
                    tmpstr = "";

                filesname[i].MoveTo(CurrentInputPath + RenameInputList[0].text + tmpstr + i + RenameInputList[1].text + filesname[i].Extension);
            }
            else if (int.Parse(RenameInputList[2].text) == 4)
            {
                if (i < 10)
                    tmpstr = "000";
                else if (i > 9 && i < 100)
                    tmpstr = "00";
                else if (i > 99 && i < 1000)
                    tmpstr = "0";
                else
                    tmpstr = "";

                filesname[i].MoveTo(CurrentInputPath + RenameInputList[0].text + tmpstr + i + RenameInputList[1].text + filesname[i].Extension);
            }
            else if (int.Parse(RenameInputList[2].text) == 5)
            {
                if (i < 10)
                    tmpstr = "0000";
                else if (i > 9 && i < 100)
                    tmpstr = "000";
                else if (i > 99 && i < 1000)
                    tmpstr = "00";
                else if (i > 999 && i < 10000)
                    tmpstr = "0";
                else
                    tmpstr = "";

                filesname[i].MoveTo(CurrentInputPath + RenameInputList[0].text + tmpstr + i + RenameInputList[1].text + filesname[i].Extension);

                
            }

            RenamePanel.gameObject.SetActive(false);

            Button[] BtnList = FileRoot.GetComponentsInChildren<Button>();
            for (int j = 0; j < BtnList.Length; j++)
            {
                Destroy(BtnList[j].gameObject);
            }

            //将参数修改为改名后的
            InputFieldList[1].text = RenameInputList[0].text + "%" + RenameInputList[2].text + "d" + RenameInputList[1].text + filesname[0].Extension;
        }
    }
    
    static bool ExecuteProgram(string exeFilename, string workDir, string args)
    {
        System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
        info.FileName = exeFilename;
        info.WorkingDirectory = workDir;
        info.UseShellExecute = true;
        info.Arguments = args;
        info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

        System.Diagnostics.Process task = null;
        bool rt = true;
        try
        {
            UnityEngine.Debug.Log("ExecuteProgram:" + args);

            task = System.Diagnostics.Process.Start(info);
            if (task != null)
            {
                task.WaitForExit(100000);
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("ExecuteProgram:" + e.ToString());
            return false;
        }
        finally
        {
            if (task != null && task.HasExited)
            {
                rt = (task.ExitCode == 0);
            }
        }

        return rt;
    }

    public void OnValueChange(string str)
    {
        if (int.Parse(RenameInputList[2].text) > 5)
            RenameInputList[2].text = 5.ToString();
        if (int.Parse(RenameInputList[2].text) < 1)
            RenameInputList[2].text = 1.ToString();

        string start = "", end = "";
        if (int.Parse(RenameInputList[2].text) == 1)
        {
            start = 1.ToString();
            end = 100.ToString();
        }
        else if (int.Parse(RenameInputList[2].text) == 2)
        {
            start = "0"+1.ToString();
            end = 100.ToString();
        }
        else if (int.Parse(RenameInputList[2].text) == 3)
        {
            start = "00" + 1.ToString();
            end = 100.ToString();
        }
        else if (int.Parse(RenameInputList[2].text) == 4)
        {
            start = "000" + 1.ToString();
            end = "0"+100.ToString();
        }
        else if (int.Parse(RenameInputList[2].text) == 5)
        {
            start = "0000" + 1.ToString();
            end = "00"+100.ToString();
        }

        textShow.text = RenameInputList[0].text + start + RenameInputList[1].text + "\n";
        textShow.text += "……" + "\n";
        textShow.text += "……" + "\n";
        textShow.text += "……" + "\n";
        textShow.text += RenameInputList[0].text + end + RenameInputList[1].text + "\n";

    }

    public void OpenFolder(string path)
    {
        var tempstr = Application.dataPath + "/../" + path + "/";
        DirectoryInfo dir = new DirectoryInfo(tempstr);
        //print(dir.FullName);
        System.Diagnostics.Process.Start("explorer.exe", dir.FullName);
    }

    //private void OnDisable()
    //{
    //    RenderTexture rt =new RenderTexture(530, 290, 0, RenderTextureFormat.ARGB32);
    //    vp.GetComponent<RawImage>().texture = rt;
    //    vp.targetTexture = rt;
    //}
}
