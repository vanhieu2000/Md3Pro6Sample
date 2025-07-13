using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace Md3Pro6Sample
{
    public partial class Md3Pro6Sample : Form
    {
        private Dictionary<int, uint> allHandle = new Dictionary<int, uint>();
        private uint currentHandle = 0;
        private int currentNodeNo = -1;

        private const string STR_SEPA = "------------------------------------";

        public Md3Pro6Sample()
        {
            InitializeComponent();

            DataTable dbItem = new DataTable("Pro6ToolDataItem");
            dbItem.Columns.AddRange(new DataColumn[]{
                                                new DataColumn("Item", typeof(uint)),
                                                new DataColumn("ForDisp", typeof(string))});

            //Set ComboBox Item
            foreach (Pro6ToolDataItem tlItem in Enum.GetValues(typeof(Pro6ToolDataItem)))
            {
                if (tlItem != Pro6ToolDataItem.Pot
                    && tlItem != Pro6ToolDataItem.Magazine)
                {
                    dbItem.Rows.Add((uint)tlItem, string.Format("{0} : {1}", (uint)tlItem, tlItem.ToString()));
                }
            }
            cmbToolDataItem.DataSource = dbItem;
            cmbToolDataItem.DisplayMember = "ForDisp";
            cmbToolDataItem.ValueMember = "Item";
            cmbToolDataItem.SelectedIndex = -1;
        }

        private void writeResult(string funcName, MMLReturn rtn, string addMsg)
        {
            string res = (rtn == MMLReturn.EM_OK) ? "(SUCCESS)" : "(FAIL)";
            string nodeStr = (currentNodeNo >= 0) ? currentNodeNo.ToString() : string.Empty;
            nodeStr = (funcName == "AllocHandle" || funcName == "FreeHandle") ? string.Empty : string.Format("Node = {1}{0}", Environment.NewLine, nodeStr);
            string resultMessage = string.Format("{0}{1}{0}<-- {2} -->{0}", Environment.NewLine, STR_SEPA, funcName)
                                   + nodeStr
                                   + string.Format("MMLReturn = {1}   {2}{0}", Environment.NewLine, rtn, res)
                                   + string.Format("{2}{1}{0}", Environment.NewLine, STR_SEPA, addMsg);

            txtResults.AppendText(resultMessage);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            //clear result textbox
            txtResults.Text = string.Format("", Environment.NewLine);
        }
        
        private void treeConnectedNodeInfo_AfterSelect(object sender, TreeViewEventArgs e)
        {
            int nodeNo;

            if (int.TryParse(e.Node.Name, out nodeNo))
            {
                currentNodeNo = nodeNo;
                currentHandle = Convert.ToUInt32(allHandle[nodeNo]);
                lblSelectedNodeInfo.Text = System.Text.RegularExpressions.Regex.Replace(
                                            treeConnectedNodeInfo.Nodes[currentNodeNo.ToString()].Text,
                                            @"\[.*\]", "").Trim();
            }
            else
            {
                currentNodeNo = -1;
                currentHandle = 0;
                lblSelectedNodeInfo.Text = "Node*(Handle:****)";
            }
        }

        private void btnAllocHandle_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            uint handle;
            string nodeInfo;
            int sendTimeout;
            int recvTimeout;
            int noop;
            byte logLevel = 3;

            nodeInfo = txtNodeInfo.Text;    //nodeInfo = nodeNo/IPAddress/tcpPort/emulate
                                            //nodeNo    ->  0-7:HSSB, 8-99:Ethernet
                                            //IPAdress  ->  Device Name or IPAdress of Machine
                                            //tcpPort   ->  Port Number(standard value: 11212)
                                            //emulate   ->  0:ReleaseMode, 1:EmulateMode

            if (nodeInfo != string.Empty
                && int.TryParse(txtSendTimeout.Text, out sendTimeout) == true
                && int.TryParse(txtReplyTimeout.Text, out recvTimeout) == true
                && int.TryParse(txtNoopCycle.Text, out noop) == true)
            {
                //**************************************************************//
                //Execute MML3 Function (md3pro6_alloc_handle)
                //**************************************************************//

                MMLReturn rtn = MMLib.md3pro6_alloc_handle(out handle, nodeInfo, sendTimeout, recvTimeout, noop, logLevel);

                //**************************************************************//

                if (rtn == MMLReturn.EM_OK)
                {
                    currentHandle = handle;
                    currentNodeNo = Convert.ToInt32(nodeInfo.Split('/')[0]);

                    if (allHandle.ContainsKey(currentNodeNo))
                    {
                        allHandle[currentNodeNo] = handle;
                        uint ct = Convert.ToUInt32(treeConnectedNodeInfo.Nodes[currentNodeNo.ToString()].Tag);
                        treeConnectedNodeInfo.Nodes[currentNodeNo.ToString()].Tag = (++ct);
                        treeConnectedNodeInfo.Nodes[currentNodeNo.ToString()].Text = string.Format("Node{0}(Handle:{1}) [{2}]", currentNodeNo, handle, ct);
                    }
                    else
                    {
                        string nodeName = string.Format("Node{0}(Handle:{1})", currentNodeNo, handle);

                        allHandle.Add(currentNodeNo, handle);
                        treeConnectedNodeInfo.Nodes.Add(currentNodeNo.ToString(), nodeName).Tag = (uint)1;
                    }
                    treeConnectedNodeInfo.SelectedNode = treeConnectedNodeInfo.Nodes[currentNodeNo.ToString()];
                }

                result = string.Format("Node = {1}{0}Send Timeout = {2}{0}Reply Timeout = {3}{0}Noop Cycle = {4}{0}Handle = {5}{0}", Environment.NewLine, nodeInfo, sendTimeout, recvTimeout, noop, (rtn == MMLReturn.EM_OK) ? handle.ToString() : "***");
                writeResult("AllocHandle", rtn, result);
            }
            else
            {
                MessageBox.Show("Input Data is not correct");
            }
        }

        private void btnFreeHandle_Click(object sender, EventArgs e)
        {
            string freeHandleInfo = string.Format("Node = {1}, Handle = {2}{0}", Environment.NewLine, currentNodeNo, currentHandle);

            //**************************************************************//
            //Execute MML3 Function (md3pro6_free_handle)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_free_handle(currentHandle);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                uint ct = Convert.ToUInt32(treeConnectedNodeInfo.Nodes[currentNodeNo.ToString()].Tag);
                if (ct > 1)
                {
                    ct--;
                    string nodeName;
                    if (ct > 1)
                        nodeName = string.Format("Node{0}(Handle:{1}) [{2}]", currentNodeNo, allHandle[currentNodeNo], ct);
                    else
                        nodeName = string.Format("Node{0}(Handle:{1})", currentNodeNo, allHandle[currentNodeNo]);

                    treeConnectedNodeInfo.Nodes[currentNodeNo.ToString()].Tag = ct;
                    treeConnectedNodeInfo.Nodes[currentNodeNo.ToString()].Text = nodeName;
                }
                else
                {
                    //Remove Node Info
                    allHandle.Remove(currentNodeNo);
                    treeConnectedNodeInfo.Nodes.RemoveByKey(currentNodeNo.ToString());
                }

                if (allHandle.Keys.Count == 0)
                {
                    currentNodeNo = -1;
                    currentHandle = 0;
                    lblSelectedNodeInfo.Text = "Node*(Handle:****)";
                }
            }

            writeResult("FreeHandle", rtn, freeHandleInfo);
        }

        private void btnGetLastError_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            int mainErr;
            int subErr;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_GetLastError)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_GetLastError(out mainErr, out subErr);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("Main Error = {1}{0}Sub Error = {2}{0}", Environment.NewLine, mainErr, subErr);
            }

            writeResult("Get Last Error", rtn, result);
        }

        private void btnCheckSystemMode_Click(object sender, EventArgs e)
        {
            string result = string.Empty;
            byte sysMode;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_chk_system_mode)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_chk_system_mode(currentHandle, out sysMode);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("SysMode = {1}{0}", Environment.NewLine, sysMode);
            }

            writeResult("Check System Mode", rtn, result);
        }

        private void btnSystemCycleStart_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            byte opeCall = (chkOpeCall.Checked ? MMLib.DLL_TRUE : MMLib.DLL_FALSE);

            //**************************************************************//
            //Execute MML3 Function (md3pro6_system_cycle_start)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_system_cycle_start(currentHandle, opeCall);

            //**************************************************************//

            result = string.Format("Ope Call = {1}{0}", Environment.NewLine, opeCall);
            writeResult("System Cycle Start", rtn, result);
        }

        private void btnCheckSystemMachFin_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            byte finish;
            uint finCondition;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_chk_system_mach_finish)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_chk_system_mach_finish(currentHandle, out finish, out finCondition);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("finish = {1}{0}finCondition = {2}{0}", Environment.NewLine, finish, finCondition);
            }
            writeResult("Check System Machining Finish", rtn, result);
        }

        private void btnCheckMcAlarm_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            uint totalAlarm, totalWarn;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_chk_mc_alarm)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_chk_mc_alarm(currentHandle, out totalAlarm, out totalWarn);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("totalAlarm = {1}{0}totalWarn = {2}{0}", Environment.NewLine, totalAlarm, totalWarn);
            }
            writeResult("Check MC Alarm", rtn, result);
        }


//=============================================================================
//      Function for TOOL DATA
//=============================================================================
        
        private void btnMaxAtcMagazine_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            uint actualMgzn = 0;
            int outMcMgzn = 0;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_max_atc_magazine)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_max_atc_magazine(currentHandle, out actualMgzn, out outMcMgzn);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("Actual Magazine = {1}{0}Out Magazine = {2}{0}", Environment.NewLine, actualMgzn, outMcMgzn);
            }

            writeResult("Max Atc Magazine", rtn, result);
        }

        private void btnAtcMagazineInfo_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            uint mgznNo = 1;//Case: Magazine Number = 1
            uint maxPot;
            int mgznType;
            uint emptyPot;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_atc_magazine_info)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_atc_magazine_info(currentHandle, mgznNo, out maxPot, out mgznType, out emptyPot);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("Magazine No. = {1}{0}Max Pot = {2}{0}Magazine Type = {3}{0}Empty Pot No. = {4}{0}", Environment.NewLine, mgznNo, maxPot, mgznType, emptyPot);
            }

            writeResult("Atc Magazine Info", rtn, result);
        }

        private void btnToolInfo_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            int inchUnit;
            uint ftnFig;
            uint itnFig;
            uint ptnFig;
            uint manageType;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_tool_info)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_tool_info(currentHandle, out inchUnit, out ftnFig, out itnFig, out ptnFig, out manageType);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("Inch Unit = {1}{0}FTN Fig. = {2}{0}ITN Fig. = {3}{0}PTN Fig. = {4}{0}Management Type = {5}{0}", Environment.NewLine, inchUnit, ftnFig, itnFig, ptnFig, manageType);
            }

            writeResult("Tool Info", rtn, result);
        }

        private void btnSpindleTool_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            uint mgznNo;
            int pot;
            uint cutter;
            uint ftn;
            uint itn;
            uint ptn;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_spindle_tool)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_spindle_tool(currentHandle, out mgznNo, out pot, out cutter, out ftn, out itn, out ptn);

            //**************************************************************//
            
            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("Magazine No. = {0}", mgznNo) + Environment.NewLine
                            + string.Format("Pot = {0}", pot) + Environment.NewLine
                            + string.Format("Cutter No. = {0}", cutter) + Environment.NewLine
                            + string.Format("FTN = {0}", ftn) + Environment.NewLine
                            + string.Format("ITN = {0}", itn) + Environment.NewLine
                            + string.Format("PTN = {0}", ptn) + Environment.NewLine;
            }

            writeResult("Spindle Tool", rtn, result);
        }

        private void btnNextTool_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            uint mgznNo;
            int potNo;
            uint cutterNo;
            uint ftn;
            uint itn;
            uint ptn;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_next_tool)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_next_tool(currentHandle, out mgznNo, out potNo, out cutterNo, out ftn, out itn, out ptn);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("Magazine No. = {1}{0}Pot No. = {2}{0}Cutter No. = {3}{0}FTN = {4}{0}ITN = {5}{0}PTN = {6}{0}", Environment.NewLine, mgznNo, potNo, cutterNo, ftn, itn, ptn);
            }

            writeResult("Next Tool", rtn, result);
        }

        private void btnTlsPotNo_Click(object sender, EventArgs e)
        {
            string result = string.Empty;

            uint mgznNo;
            int potNo;
            uint tlsNo = 1;//Case: TLS Number = 1

            //**************************************************************//
            //Execute MML3 Function (md3pro6_tls_potno)
            //**************************************************************//

            MMLReturn rtn = MMLib.md3pro6_tls_potno(currentHandle, tlsNo, out mgznNo, out potNo);

            //**************************************************************//

            if (rtn == MMLReturn.EM_OK)
            {
                result = string.Format("TLS No. = {1}{0}Magazine No. = {2}{0}Pot = {3}{0}", Environment.NewLine, tlsNo, mgznNo, potNo);
            }

            writeResult("Tls Pot No", rtn, result);
        }

        private void btnGetToolDataItem_Click(object sender, EventArgs e)
        {
            int pot = 0;
            List<int> allPotNo = new List<int>();
            string result = string.Empty;

            uint item;
            uint[] mgznNo;
            int[] potNo;
            int[] value;
            uint sumArray;

            //Tool No1
            if (int.TryParse(txtToolDataPotNo1.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            //Tool No2
            if (int.TryParse(txtToolDataPotNo2.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            //Tool No3
            if (int.TryParse(txtToolDataPotNo3.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            //Tool No4
            if (int.TryParse(txtToolDataPotNo4.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            //Tool No5
            if (int.TryParse(txtToolDataPotNo5.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            if (allPotNo.Count > 0 && (cmbToolDataItem.SelectedIndex >= 0))
            {
                item = (uint)cmbToolDataItem.SelectedValue;
                potNo = allPotNo.ToArray();
                sumArray = (uint)allPotNo.Count;
                mgznNo = new uint[sumArray];
                value = new int[sumArray];

                for (int i = 0; i < sumArray; i++)
                {
                    mgznNo[i] = 1;
                    value[i] = 0;
                }

                //**************************************************************//
                //Execute MML3 Function (md3pro6_get_tool_data_item)
                //**************************************************************//

                MMLReturn rtn = MMLib.md3pro6_get_tool_data_item(currentHandle, item, mgznNo, potNo, value, sumArray);

                //**************************************************************//

                for (int i = 0; i < value.Length; i++)
                {
                    result = result + string.Format("Pot = {1}, {3} = {2}{0}", Environment.NewLine, potNo[i], value[i], (Pro6ToolDataItem)item);
                }

                writeResult("Get Tool Data Item", rtn, result);
            }
            else
            {
                MessageBox.Show("Input Data is not correct.");
            }
        }

        private void btnSetToolDataItem_Click(object sender, EventArgs e)
        {
            int pot = 0;
            int setValue = 0;
            List<int> allPotNo = new List<int>();
            List<int> allSetValue = new List<int>();
            string result = string.Empty;

            uint item;
            uint[] mgznNo;
            int[] potNo;
            int[] value;
            uint sumArray;

            //Tool No1
            if (int.TryParse(txtToolDataPotNo1.Text, out pot)
                && int.TryParse(txtToolDataSetValue1.Text, out setValue))
            {
                allPotNo.Add(pot);
                allSetValue.Add(setValue);
            }

            //Tool No2
            if (int.TryParse(txtToolDataPotNo2.Text, out pot)
                && int.TryParse(txtToolDataSetValue2.Text, out setValue))
            {
                allPotNo.Add(pot);
                allSetValue.Add(setValue);
            }

            //Tool No3
            if (int.TryParse(txtToolDataPotNo3.Text, out pot)
                && int.TryParse(txtToolDataSetValue3.Text, out setValue))
            {
                allPotNo.Add(pot);
                allSetValue.Add(setValue);
            }

            //Tool No4
            if (int.TryParse(txtToolDataPotNo4.Text, out pot)
                && int.TryParse(txtToolDataSetValue4.Text, out setValue))
            {
                allPotNo.Add(pot);
                allSetValue.Add(setValue);
            }

            //Tool No5
            if (int.TryParse(txtToolDataPotNo5.Text, out pot)
                && int.TryParse(txtToolDataSetValue5.Text, out setValue))
            {
                allPotNo.Add(pot);
                allSetValue.Add(setValue);
            }

            if (allPotNo.Count > 0 && (cmbToolDataItem.SelectedIndex >= 0))
            {
                item = (uint)cmbToolDataItem.SelectedValue;
                potNo = allPotNo.ToArray();
                sumArray = (uint)allPotNo.Count;
                mgznNo = new uint[sumArray];
                value = allSetValue.ToArray();

                for (int i = 0; i < sumArray; i++)
                {
                    mgznNo[i] = 1;
                }

                //**************************************************************//
                //Execute MML3 Function (md3pro6_set_tool_data_item)
                //**************************************************************//

                MMLReturn rtn = MMLib.md3pro6_set_tool_data_item(currentHandle, item, mgznNo, potNo, value, sumArray);

                //**************************************************************//

                for (int i = 0; i < value.Length; i++)
                {
                    result = result + string.Format("Pot = {1}, {3} = {2}{0}", Environment.NewLine, potNo[i], value[i], (Pro6ToolDataItem)item);
                }

                writeResult("Set Tool Data Item", rtn, result);
            }
            else
            {
                MessageBox.Show("Input Data is not correct.");
            }
        }

        private void btnGetCutterDataItem_Click(object sender, EventArgs e)
        {
            int pot = 0;
            uint cutter = 0;
            List<int> allPotNo = new List<int>();
            List<uint> allCutterNo = new List<uint>();
            string result = string.Empty;

            uint item;
            uint[] mgznNo;
            int[] potNo;
            uint[] cutterNo;
            int[] value;
            uint sumArray;

            //Tool No1
            if (int.TryParse(txtToolDataPotNo1.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo1.Text, out cutter))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
            }

            //Tool No2
            if (int.TryParse(txtToolDataPotNo2.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo2.Text, out cutter))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
            }

            //Tool No3
            if (int.TryParse(txtToolDataPotNo3.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo3.Text, out cutter))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
            }

            //Tool No4
            if (int.TryParse(txtToolDataPotNo4.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo4.Text, out cutter))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
            }

            //Tool No5
            if (int.TryParse(txtToolDataPotNo5.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo5.Text, out cutter))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
            }

            if (allPotNo.Count > 0 && (cmbToolDataItem.SelectedIndex >= 0))
            {
                item = (uint)cmbToolDataItem.SelectedValue;
                potNo = allPotNo.ToArray();
                cutterNo = allCutterNo.ToArray();
                sumArray = (uint)allPotNo.Count;
                mgznNo = new uint[sumArray];
                value = new int[sumArray];

                for (int i = 0; i < sumArray; i++)
                {
                    mgznNo[i] = 1;
                    value[i] = 0;
                }

                //**************************************************************//
                //Execute MML3 Function (md3pro6_get_cutter_data_item)
                //**************************************************************//

                MMLReturn rtn = MMLib.md3pro6_get_cutter_data_item(currentHandle, item, mgznNo, potNo, cutterNo, value, sumArray);

                //**************************************************************//

                for (int i = 0; i < value.Length; i++)
                {
                    result = result + string.Format("Pot = {1}, Cutter = {2}, {4} = {3}{0}", Environment.NewLine, potNo[i], cutterNo[i], value[i], (Pro6ToolDataItem)item);
                }

                writeResult("Get Cutter Data Item", rtn, result);
            }
            else
            {
                MessageBox.Show("Input Data is not correct.");
            }
        }

        private void btnSetCutterDataItem_Click(object sender, EventArgs e)
        {
            int pot = 0;
            uint cutter = 0;
            int setValue = 0;
            List<int> allPotNo = new List<int>();
            List<uint> allCutterNo = new List<uint>();
            List<int> allSetValue = new List<int>();
            string result = string.Empty;

            uint item;
            uint[] mgznNo;
            int[] potNo;
            uint[] cutterNo;
            int[] value;
            uint sumArray;

            //Tool No1
            if (int.TryParse(txtToolDataPotNo1.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo1.Text, out cutter)
                && int.TryParse(txtToolDataSetValue1.Text, out setValue))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
                allSetValue.Add(setValue);
            }

            //Tool No2
            if (int.TryParse(txtToolDataPotNo2.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo2.Text, out cutter)
                && int.TryParse(txtToolDataSetValue2.Text, out setValue))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
                allSetValue.Add(setValue);
            }

            //Tool No3
            if (int.TryParse(txtToolDataPotNo3.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo3.Text, out cutter)
                && int.TryParse(txtToolDataSetValue3.Text, out setValue))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
                allSetValue.Add(setValue);
            }

            //Tool No4
            if (int.TryParse(txtToolDataPotNo4.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo4.Text, out cutter)
                && int.TryParse(txtToolDataSetValue4.Text, out setValue))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
                allSetValue.Add(setValue);
            }

            //Tool No5
            if (int.TryParse(txtToolDataPotNo5.Text, out pot)
                && uint.TryParse(txtToolDataCutterNo5.Text, out cutter)
                && int.TryParse(txtToolDataSetValue5.Text, out setValue))
            {
                allPotNo.Add(pot);
                allCutterNo.Add(cutter);
                allSetValue.Add(setValue);
            }

            if (allPotNo.Count > 0 && (cmbToolDataItem.SelectedIndex >= 0))
            {
                item = (uint)cmbToolDataItem.SelectedValue;
                potNo = allPotNo.ToArray();
                cutterNo = allCutterNo.ToArray();
                sumArray = (uint)allPotNo.Count;
                mgznNo = new uint[sumArray];
                value = allSetValue.ToArray();

                for (int i = 0; i < sumArray; i++)
                {
                    mgznNo[i] = 1;
                }

                //**************************************************************//
                //Execute MML3 Function (md3pro6_get_cutter_data_item)
                //**************************************************************//

                MMLReturn rtn = MMLib.md3pro6_set_cutter_data_item(currentHandle, item, mgznNo, potNo, cutterNo, value, sumArray);

                //**************************************************************//


                for (int i = 0; i < value.Length; i++)
                {
                    result = result + string.Format("Pot = {1}, Cutter = {2}, {4} = {3}{0}", Environment.NewLine, potNo[i], cutterNo[i], value[i], (Pro6ToolDataItem)item);
                }

                writeResult("Set Cutter Data Item", rtn, result);
            }
            else
            {
                MessageBox.Show("Input Data is not correct.");
            }
        }

        private void btnClearToolData_Click(object sender, EventArgs e)
        {
            int pot = 0;
            List<int> allPotNo = new List<int>();
            string message = string.Empty;

            uint[] mgznNo;
            int[] potNo;
            uint sumArray;

            //Tool No1
            if (int.TryParse(txtToolDataPotNo1.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            //Tool No2
            if (int.TryParse(txtToolDataPotNo2.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            //Tool No3
            if (int.TryParse(txtToolDataPotNo3.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            //Tool No4
            if (int.TryParse(txtToolDataPotNo4.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            //Tool No5
            if (int.TryParse(txtToolDataPotNo5.Text, out pot))
            {
                allPotNo.Add(pot);
            }

            if (allPotNo.Count > 0)
            {
                potNo = allPotNo.ToArray();
                sumArray = (uint)allPotNo.Count;
                mgznNo = new uint[sumArray];

                for (int i = 0; i < sumArray; i++)
                {
                    mgznNo[i] = 1;
                }

                //**************************************************************//
                //Execute MML3 Function (md3pro6_clear_tool_data)
                //**************************************************************//

                MMLReturn rtn = MMLib.md3pro6_clear_tool_data(currentHandle, mgznNo, potNo, sumArray);

                //**************************************************************//

                for (int i = 0; i < potNo.Length; i++)
                {
                    message = message + string.Format("Pot = {1}{0}", Environment.NewLine, potNo[i]);
                }

                writeResult("Clear Tool Data", rtn, message);
            }
            else
            {
                MessageBox.Show("Input Data is not correct.");
            }
        }

        private void btnClearAllInputArea_Click(object sender, EventArgs e)
        {
            txtToolDataPotNo1.Text = txtToolDataPotNo2.Text
                                   = txtToolDataPotNo3.Text
                                   = txtToolDataPotNo4.Text
                                   = txtToolDataPotNo5.Text
                                   = string.Empty;

            txtToolDataCutterNo1.Text = txtToolDataCutterNo2.Text
                                      = txtToolDataCutterNo3.Text
                                      = txtToolDataCutterNo4.Text
                                      = txtToolDataCutterNo5.Text
                                      = string.Empty;

            cmbToolDataItem.SelectedIndex = -1;

            txtToolDataSetValue1.Text = txtToolDataSetValue2.Text
                                      = txtToolDataSetValue3.Text
                                      = txtToolDataSetValue4.Text
                                      = txtToolDataSetValue5.Text
                                      = string.Empty;
        }

        private void btnGetMcAlarmNumber_Click(object sender, EventArgs e)
        {
            string result = string.Empty;
            uint maxAlarm = 0;

            //**************************************************************//
            //Execute MML3 Function (md3pro6_max_alarm_history)
            //**************************************************************//
            MMLReturn rtn = MMLib.md3pro6_max_alarm_history(currentHandle, out maxAlarm);

            if (rtn == MMLReturn.EM_OK && maxAlarm > 0)
            {
                uint[] alarmNo = new uint[maxAlarm];                
                byte[] alarmType = new byte[maxAlarm];                
                byte[] seriousLevel = new byte[maxAlarm];
                byte[] poutDisable = new byte[maxAlarm];
                byte[] cycleStartDisable = new byte[maxAlarm];
                byte[] retryDisable = new byte[maxAlarm];                
                byte[] failedNCReset = new byte[maxAlarm];
                SystemTime[] alarmDate = new SystemTime[maxAlarm];
                SystemTime[] resetDate = new SystemTime[maxAlarm];
                uint arraySize = maxAlarm;
                StringBuilder ncMessage = null;
                
                //**************************************************************//
                //Execute MML3 Function (md3pro6_mc_alarm)
                //**************************************************************//
                rtn = MMLReturn.EM_INTERNAL;
                //rtn = MMLib.md3pro6_mc_alarm(currentHandle, alarmNo, alarmType, seriousLevel, poutDisable, cycleStartDisable, 
                //    retryDisable, failedNCReset, alarmDate, ref arraySize, ncMessage);

                rtn = MMLib.md3pro6_mc_alarm_history(currentHandle, alarmNo, alarmType, seriousLevel, retryDisable, 
                        alarmDate, resetDate, ref arraySize, ncMessage);

                //**************************************************************//

                if (rtn == MMLReturn.EM_OK)
                {
                    result = string.Format("alarmNo = {1}{0}ncMessage = {2}{0}", Environment.NewLine, alarmNo[0], ncMessage);
                }
                writeResult("Get MC Alarm", rtn, result);
            }

            
        }

    }
}
