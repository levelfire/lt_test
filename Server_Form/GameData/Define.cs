using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Server_Form.GameData;

namespace Server_Form
{
    class Define
    {
        public static int initialCapacity = 100;
        public static int numProcs = Environment.ProcessorCount;
        public static int concurrencyLevel = numProcs * 2;

        private static XmlDocument xmlDocStage = new XmlDocument();
        private static XmlDocument xmlDocStep = new XmlDocument();
        private static XmlDocument xmlDocSubStep = new XmlDocument();
        private static string OpenFilePath = @"..\stages";

        private static string RootNodeName = "root";

        //private static string StageNodeName = "//stage";
        //private static string StepNodeName = "//step";
        //private static string SubStepNodeName = "//substep";

        //private static string TimeNodeName = "//time";
        //private static string MaxNodeName = "//maxdiff";
        //private static string DiffNodeName = "//diff";

        private static string StageNodeName = "stage";
        private static string StepNodeName = "step";
        private static string SubStepNodeName = "substep";

        private static string TimeNodeName = "time";
        private static string MaxNodeName = "maxdiff";
        private static string DiffNodeName = "diff";

        private static string BossNodeName = "boss";

        public static void LoadFile()                           //参数为指定的目录
        {
            //在指定目录及子目录下查找文件,在listBox1中列出子目录及文件
            DirectoryInfo Dir = new DirectoryInfo(OpenFilePath);
            try
            {
                foreach (DirectoryInfo stage in Dir.GetDirectories())     //查找子目录   
                {
                    if ("." == stage.ToString() || ".." == stage.ToString()
                        || -1 != stage.Attributes.ToString().IndexOf("Hidden"))
                    {
                        continue;
                    }
                    string tDir = OpenFilePath + "\\" + stage.ToString();
                    xmlDocStage.Load(tDir + @"\stage.xml");
                    XmlNode rootnode = xmlDocStage.SelectSingleNode(RootNodeName);
                    XmlNode stagenode = rootnode.SelectSingleNode(StageNodeName);
                    //XmlNodeList nodes = rootnode.SelectNodes(StageNodeName);

                    StageData tstage = new StageData();
                    tstage.ID = System.Convert.ToInt32(stage.ToString());
                    //Data_Xml.SwanpPos_H.Add(stage.ID, stage);

                    foreach (DirectoryInfo step in stage.GetDirectories())     //查找子目录   
                    {
                        if ("." == step.ToString() || ".." == step.ToString()
                        || -1 != step.Attributes.ToString().IndexOf("Hidden"))
                        {
                            continue;
                        }
                        //FindFile(Dir + d.ToString() + "\\");
                        //listBox1.Items.Add(Dir + d.ToString() + "\\");       //listBox1中填加目录名
                        string tDirS = tDir + "\\" + step.ToString();
                        xmlDocStep.Load(tDirS + @"\step.xml");
                        int index = System.Convert.ToInt32(step.ToString());

                        XmlNode srootnode = xmlDocStep.SelectSingleNode(RootNodeName);
                        XmlNode stepnode = srootnode.SelectSingleNode(StepNodeName);
                        XmlNode steptimenode = stepnode.SelectSingleNode(TimeNodeName);

                        StepData tstep = new StepData();
                        tstep.ID = index;
                        tstep.Time = System.Convert.ToInt32(steptimenode.InnerText);

                        foreach (DirectoryInfo substep in step.GetDirectories())     //查找子目录   
                        {
                            if ("." == substep.ToString() || ".." == substep.ToString()
                                || -1 != substep.Attributes.ToString().IndexOf("Hidden"))
                            {
                                continue;
                            }
                            string tDirSubstep = tDirS + "\\" + substep.ToString();
                            xmlDocSubStep.Load(tDirSubstep + @"\substep.xml");
                            int substepIndex = System.Convert.ToInt32(substep.ToString()); ;

                            XmlNode substeprootnode = xmlDocSubStep.SelectSingleNode(RootNodeName);
                            XmlNode substepnode = substeprootnode.SelectSingleNode(SubStepNodeName);

                            SubStepData tsubstep = new SubStepData();
                            tsubstep.ID = substepIndex++;
                            tsubstep.MaxDiff = System.Convert.ToInt32(substepnode.SelectSingleNode(MaxNodeName).InnerText);

                            XmlNode bossnode = substepnode.SelectSingleNode(BossNodeName);

                            if (null != bossnode)
                            {
                                int nBossID = System.Convert.ToInt32(bossnode.SelectSingleNode("bossid").InnerText);
                                BossData.init(nBossID,
                                            System.Convert.ToInt32(bossnode.SelectSingleNode("fireloop").InnerText));
        
                                tsubstep.m_BossID = nBossID;
                            }
                            else
                            {
                                tsubstep.m_BossID = 0;
                            }

                            XmlNodeList diffnodes = substepnode.SelectNodes(DiffNodeName);

                            int nDiffindex = 1;
                            foreach (XmlNode diff in diffnodes)
                            {
                                DiffData tdiff = new DiffData();

                                tdiff.ID = nDiffindex++;
                                tdiff.PosX = System.Convert.ToInt32(diff.SelectSingleNode("posx").InnerText);
                                tdiff.PosY = System.Convert.ToInt32(diff.SelectSingleNode("posy").InnerText);
                                tdiff.LeftX = System.Convert.ToInt32(diff.SelectSingleNode("leftx").InnerText);
                                tdiff.LeftY = System.Convert.ToInt32(diff.SelectSingleNode("lefty").InnerText);
                                tdiff.RightX = System.Convert.ToInt32(diff.SelectSingleNode("rightx").InnerText);
                                tdiff.RightY = System.Convert.ToInt32(diff.SelectSingleNode("righty").InnerText);

                                tsubstep.DiffList.TryAdd(tdiff.ID, tdiff);
                            }

                            XmlNodeList monsternodes = substepnode.SelectNodes("monster");
                            int nMonsterindex = 1;
                            MonsterData.init();
                            foreach (XmlNode monsternode in monsternodes)
                            {
                                //int nMonsterID = Convert.ToInt32(monsternode.Attributes["ID"].Value);
                                int nMonsterID = System.Convert.ToInt32(monsternode.SelectSingleNode("id").InnerText);
                                if (MonsterData.MonsterDataList.ContainsKey(nMonsterID))
                                {
                                    MonsterData md = MonsterData.MonsterDataList[nMonsterID];
                                    tsubstep.MonsterList.TryAdd(nMonsterindex, md);
                                    nMonsterindex++;
                                }
                            }

                            tstep.SubStepList.TryAdd(tsubstep.ID, tsubstep);
                        }

                        ////遍历substep
                        //XmlNodeList subnodes = stepnode.SelectNodes(SubStepNodeName);
                        //int i = 1;
                        //foreach (XmlNode sub in subnodes)
                        //{
                        //    SubStepData tsubstep = new SubStepData();
                        //    tsubstep.Index = i++;
                        //    tsubstep.MaxDiff = System.Convert.ToInt32(sub.SelectSingleNode(MaxNodeName).InnerText);

                        //    XmlNodeList diffnodes = sub.SelectNodes(DiffNodeName);
                        //    int nDiffindex = 1;
                        //    foreach (XmlNode diff in diffnodes)
                        //    {
                        //        DiffData tdiff = new DiffData();

                        //        tdiff.Index = nDiffindex++;
                        //        tdiff.PosX = System.Convert.ToInt32(diff.SelectSingleNode("posx").InnerText);
                        //        tdiff.PosY = System.Convert.ToInt32(diff.SelectSingleNode("posy").InnerText);
                        //        tdiff.LeftX = System.Convert.ToInt32(diff.SelectSingleNode("leftx").InnerText);
                        //        tdiff.LeftY = System.Convert.ToInt32(diff.SelectSingleNode("lefty").InnerText);
                        //        tdiff.RightX = System.Convert.ToInt32(diff.SelectSingleNode("rightx").InnerText);
                        //        tdiff.RightY = System.Convert.ToInt32(diff.SelectSingleNode("righty").InnerText);

                        //        tsubstep.DiffList.TryAdd(tdiff.Index, tdiff);
                        //    }
                        //    tstep.SubStepList.TryAdd(tsubstep.Index, tsubstep);
                        //}

                        tstage.StepList.TryAdd(tstep.ID, tstep);
                    }
                    StageData.StageList.TryAdd(tstage.ID,tstage);

                }
                //foreach (FileInfo f in Dir.GetFiles("*.*"))             //查找文件
                //{
                //    listBox1.Items.Add(Dir + f.ToString());     //listBox1中填加文件名
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }
    }
}
