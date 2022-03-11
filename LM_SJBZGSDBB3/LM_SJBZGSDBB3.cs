using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM_SJBZGSDBB3
{
    [HotUpdate]
    public class LM_SJBZGSDBB3 : AbstractDynamicFormPlugIn
    {
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            //1.根据单据头条件 查询对应生产入库单 物料的数量  放在本月完工数量上  
            //2 在根据物料获取对应BOM的直接人工标准工时
            //3 在根据条件去查询工时管理表 的数据
            if (e.Key.EqualsIgnoreCase("F_ora_Button"))
            {
                string sccj = "";
                string sccjtj = "";
                string sccjtj1 = "";
                string Material = "";
                string Materialtj = "";
                string Materialtj1 = "";
                string YMb = "";
                string YMe = "";
                string YMbtj = "";
                string YMetj = "";
                string yearmonth = "";
                string yearmonth2 = "";
                string year = this.Model.GetValue("F_ora_Year").ToString();
                string month = this.Model.GetValue("F_ora_Month").ToString();

                //年月都为空 则查询所有记录
                if (Convert.ToInt32(this.Model.GetValue("F_ora_Year")) == 0 && Convert.ToInt32(this.Model.GetValue("F_ora_Month")) == 0)
                {
                    YMbtj = "";
                    YMetj = "";
                }
                //年不为空 月为空 查询当年记录
                else if (Convert.ToInt32(this.Model.GetValue("F_ora_Year")) >= 0 && Convert.ToInt32(this.Model.GetValue("F_ora_Month")) == 0)
                {
                    YMb = year + "-" + 01 + "-" + "01";
                    YMe = year + "-" + 12 + "-" + "31";
                    YMbtj = $"where FDate >='{YMb}'";
                    YMetj = $"and FDate <= '{YMe}'";
                    yearmonth = $" where w.F_ora_ReportDate >= '{YMb}'";
                    yearmonth2 = $"and w.F_ora_ReportDate <= '{YMe}'";
                }
                //年为空 月不为空 提示报错
                else if (Convert.ToInt32(this.Model.GetValue("F_ora_Year")) == 0 && Convert.ToInt32(this.Model.GetValue("F_ora_Month")) >= 0)
                {
                    this.View.ShowMessage("请重新填写年月");
                }
                //年不为空 月不为空  查询该年 月
                else if (Convert.ToInt32(this.Model.GetValue("F_ora_Year")) >= 0 && Convert.ToInt32(this.Model.GetValue("F_ora_Month")) >= 0)
                {
                    //当为二月的时候
                    if (Convert.ToInt32(this.Model.GetValue("F_ora_Month")) == 2)
                    {
                        YMb = year + "-" + month + "-" + "01";
                        YMe = year + "-" + month + "-" + "28";
                        YMbtj = $"where i.FDate >='{YMb}'";
                        YMetj = $"and i.FDate <= '{YMe}'";
                        yearmonth = $"where w.F_ora_ReportDate >= '{YMb}'";
                        yearmonth2 = $"and w.F_ora_ReportDate <= '{YMe}'";
                    }
                    else
                    {
                        YMb = year + "-" + month + "-" + "01";
                        YMe = year + "-" + month + "-" + "31";
                        YMbtj = $"where i.FDate >='{YMb}'";
                        YMetj = $"and i.FDate <= '{YMe}'";
                        yearmonth = $"where w.F_ora_ReportDate >= '{YMb}'";
                        yearmonth2 = $"and w.F_ora_ReportDate <= '{YMe}'";
                    }
                }
                //当生产车间不为空
                if (this.Model.GetValue("F_ORA_PRODUCTIONWORKSHOP").IsNullOrEmptyOrWhiteSpace() == false)
                {
                    DynamicObject SCCJ = this.Model.GetValue("F_ORA_PRODUCTIONWORKSHOP") as DynamicObject;
                    sccj = SCCJ[0].ToString(); //生产车间
                    sccjtj = $"and F_ORA_PRODUCTIONWORKSHOP='{sccj}'";
                    sccjtj1 = $"and CHEJIAN='{sccj}'";
                }
                //物料不为空
                if (this.Model.GetValue("F_ora_Material").IsNullOrEmptyOrWhiteSpace() == false)
                {
                    DynamicObject WL = this.Model.GetValue("F_ora_Material") as DynamicObject;
                    Material = WL[0].ToString(); //物料
                    Materialtj = $"and F_ORA_MATERIALID='{Material}'";
                    Materialtj1 = $" and ie.FMATERIALID='{Material}'";
                }
                //CONVERT(varchar(6),isnull(a.FDate,b.FDate),112)  as FDate  FDate, ,w.F_ora_ReportDate as FDate
                string sql = $@" select  
                            isnull(a.FMATERIALID,b.F_ORA_MATERIALID) as FMATERIALID
                            ,isnull(a.FNAME,b.FMaterialName)as FNAME
                            ,isnull(a.FSPECIFICATION,b.FSPECIFICATION)as FSPECIFICATION
                            ,b.FUnitName as FUnit
                            ,isnull(a.FWORKSHOPID,b.F_ORA_PRODUCTIONWORKSHOP)as FWORKSHOPID
                            ,isnull(a.FQTY,0)as FQTY
                            ,isnull(a.F_ORA_SUMWORKHOURQTY,0)as F_ORA_SUMWORKHOURQTY
                            ,isnull(b.F_ORA_PRODUCTIONTIME,0)as F_ORA_PRODUCTIONTIME
                            ,isnull(b.F_ORA_CLEANTIME,0)as F_ORA_CLEANTIME
                            ,isnull(b.F_ORA_WEEKMONTHTIME,0)as F_ORA_WEEKMONTHTIME
                            ,isnull(b.F_ORA_ALLWORKHOURS,0)as F_ORA_ALLWORKHOURS
                            from (
                                select   distinct
                                ie.FMATERIALID as FMATERIALID
                                ,ml.FNAME AS FNAME
                                ,FSPECIFICATION AS FSPECIFICATION
                                ,unl.FNAME as funlName
                                ,sum(FREALQTY) as FQTY 
                                , F_ORA_SUMWORKHOURQTY
                                ,ie.FWORKSHOPID
                                from T_PRD_INSTOCKENTRY ie
                                join T_PRD_INSTOCK i on i.FID = ie.FID 
                                join T_PRD_MO MO ON ie.FMOBILLNO = mo.FBILLNO
                                join T_BD_MATERIAL_L ml on ml.FMATERIALID = ie.FMATERIALID
                                join t_BD_MaterialBase mb on mb.FMATERIALID = ie.FMATERIALID
                                join T_ENG_BOM bom on bom.FMATERIALID = ie.FMATERIALID
                                join T_BD_UNIT un on un.FUNITID = mb.FBASEUNITID
                                join T_BD_UNIT_L unl on un.FUNITID = unl.FUNITID
                             {YMbtj} {YMetj}{sccjtj1}{Materialtj1} 
                            group by ie.FMATERIALID,ml.FNAME,FSPECIFICATION,unl.FNAME,ie.FWORKSHOPID,F_ORA_SUMWORKHOURQTY,bom.FNUMBER
                            )a full join 
                            (  select F_ORA_MATERIALID ,F_ORA_PRODUCTIONWORKSHOP, sum(F_ORA_PRODUCTIONTIME) as F_ORA_PRODUCTIONTIME  ,sum(F_ORA_CLEANTIME) as F_ORA_CLEANTIME ,sum(F_ORA_WEEKMONTHTIME) as F_ORA_WEEKMONTHTIME ,sum(F_ORA_ALLWORKHOURS) as  F_ORA_ALLWORKHOURS ,ml.FNAME as FMaterialName,FSPECIFICATION,unl.FNAME as FUnitName
                            from T_LMYD_WorkHoursEntry we
                            join T_LMYD_WorkHours  w on w.FID = we.FID
                            join T_BD_MATERIAL_L ml on ml.FMATERIALID = we.F_ORA_MATERIALID
                            join t_BD_MaterialBase mb on mb.FMATERIALID = we.F_ORA_MATERIALID
                            join T_BD_UNIT un on un.FUNITID = mb.FBASEUNITID
                            join T_BD_UNIT_L unl on un.FUNITID = unl.FUNITID
                            {yearmonth} {yearmonth2} {Materialtj} {sccjtj}
                            and w.FDOCUMENTSTATUS = 'c'
                           group by F_ORA_MATERIALID,F_ORA_PRODUCTIONWORKSHOP,ml.FNAME,FSPECIFICATION,unl.FNAME 
                            )b
                            on a.FMATERIALID = b.F_ORA_MATERIALID and a.FWORKSHOPID = b.F_ORA_PRODUCTIONWORKSHOP";
                DataSet ds = DBServiceHelper.ExecuteDataSet(this.Context, sql);//where F_ORA_PRODUCTIONWORKSHOP<>0
                DataTable dt = ds.Tables[0];
                
                var rEntity = this.View.Model.BusinessInfo.GetEntity("FEntity");

                this.View.Model.DeleteEntryData("FEntity");
                if (dt.Rows.Count>0) 
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        this.Model.CreateNewEntryRow(rEntity, i);
                        //string FDate = dt.Rows[i]["FDate"].ToString(); //日期
                        string  FMATERIALID = dt.Rows[i]["FMATERIALID"].ToString();//物料编码
                        string FNAME = dt.Rows[i]["FNAME"].ToString();//物料名称
                        string FSPECIFICATION = dt.Rows[i]["FSPECIFICATION"].ToString();//规格型号
                        string FUnit = dt.Rows[i]["FUnit"].ToString();//单位
                        string FWORKSHOPID = dt.Rows[i]["FWORKSHOPID"].ToString();//车间编码
                        decimal FQTY =Convert.ToDecimal(dt.Rows[i]["FQTY"].ToString());//获取本月完工数量
                        decimal FSUMWORKHOURS = Convert.ToDecimal(dt.Rows[i]["F_ORA_SUMWORKHOURQTY"].ToString());//单位标准工时
                        decimal BZZGS = FQTY * FSUMWORKHOURS;//标准总工时
                        string SCSJ = dt.Rows[i]["F_ORA_PRODUCTIONTIME"].ToString();//生产时间
                        string RCQJSJ = dt.Rows[i]["F_ORA_CLEANTIME"].ToString();//日常清洁时间
                        string BYSJ = dt.Rows[i]["F_ORA_WEEKMONTHTIME"].ToString();//周保养/月保养时间
                        decimal ZGS = Convert.ToDecimal(dt.Rows[i]["F_ORA_ALLWORKHOURS"].ToString());//总工时
                        decimal CYGS = BZZGS - ZGS;//差异工时

                        //this.Model.SetValue("F_ora_Date", FDate,i);//日期
                        this.Model.SetValue("F_ORA_MATERIALID", FMATERIALID, i);//物料编码
                        this.Model.SetValue("F_ora_MaterialName", FNAME, i);//物料名称
                        this.Model.SetValue("F_ora_SpecificalOrdNum", FSPECIFICATION, i);//规格型号
                        this.Model.SetValue("F_ora_Unit", FUnit, i);//单位
                        this.Model.SetValue("F_ora_SCCJ", FWORKSHOPID, i);//生产车间
                        this.Model.SetValue("F_ORA_MONTHCOMPQTY", FQTY, i);//赋值本月完工数量
                        this.Model.SetValue("F_ORA_UNITSTANDWORKHOUR", FSUMWORKHOURS, i);//赋值本月完工数量

                        this.Model.SetValue("F_ORA_DIFFERENTIALWORKHOUR", CYGS, i);//差异工时
                        this.Model.SetValue("F_ORA_TOTALWORKHOURS", BZZGS, i);
                        this.Model.SetValue("F_ORA_PRODUCTIONTIME", SCSJ, i);//生产时间
                        this.Model.SetValue("F_ORA_DAILYCLEANTIME", RCQJSJ, i);//日常清洁时间
                        this.Model.SetValue("F_ORA_WEEKMONTHMAIN", BYSJ, i);//保养时间
                        this.Model.SetValue("F_ORA_ACTUALWORKHOURS", ZGS, i);//实际总工时
                    }
                    
                    this.Model.ClearNoDataRow();
                    this.View.UpdateView("FEntity");
                }
               
            }
        }
    }
}
