//using System.Xml;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Search;
using System.Collections.Generic;

namespace Lemorange.Modules.FinHubAddOns.Components
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Controller class for FinHubAddOns
    /// 
    /// The FeatureController class is defined as the BusinessController in the manifest file (.dnn)
    /// DotNetNuke will poll this class to find out which Interfaces the class implements. 
    /// 
    /// The IPortable interface is used to import/export content from a DNN module
    /// 
    /// The ISearchable interface is used by DNN to index the content of a module
    /// 
    /// The IUpgradeable interface allows module developers to execute code during the upgrade 
    /// process for a module.
    /// 
    /// Below you will find stubbed out implementations of each, uncomment and populate with your own data
    /// </summary>
    /// -----------------------------------------------------------------------------

    //uncomment the interfaces to add the support.
    public class FeatureController //: IPortable, ISearchable, IUpgradeable
    {
        #region Optional Interfaces

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ExportModule implements the IPortable ExportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be exported</param>
        /// -----------------------------------------------------------------------------
        //public string ExportModule(int ModuleID)
        //{
        //string strXML = "";

        //List<FinHubAddOnsInfo> colFinHubAddOnss = GetFinHubAddOnss(ModuleID);
        //if (colFinHubAddOnss.Count != 0)
        //{
        //    strXML += "<FinHubAddOnss>";

        //    foreach (FinHubAddOnsInfo objFinHubAddOns in colFinHubAddOnss)
        //    {
        //        strXML += "<FinHubAddOns>";
        //        strXML += "<content>" + DotNetNuke.Common.Utilities.XmlUtils.XMLEncode(objFinHubAddOns.Content) + "</content>";
        //        strXML += "</FinHubAddOns>";
        //    }
        //    strXML += "</FinHubAddOnss>";
        //}

        //return strXML;

        //	throw new System.NotImplementedException("The method or operation is not implemented.");
        //}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ImportModule implements the IPortable ImportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be imported</param>
        /// <param name="Content">The content to be imported</param>
        /// <param name="Version">The version of the module to be imported</param>
        /// <param name="UserId">The Id of the user performing the import</param>
        /// -----------------------------------------------------------------------------
        //public void ImportModule(int ModuleID, string Content, string Version, int UserID)
        //{
        //XmlNode xmlFinHubAddOnss = DotNetNuke.Common.Globals.GetContent(Content, "FinHubAddOnss");
        //foreach (XmlNode xmlFinHubAddOns in xmlFinHubAddOnss.SelectNodes("FinHubAddOns"))
        //{
        //    FinHubAddOnsInfo objFinHubAddOns = new FinHubAddOnsInfo();
        //    objFinHubAddOns.ModuleId = ModuleID;
        //    objFinHubAddOns.Content = xmlFinHubAddOns.SelectSingleNode("content").InnerText;
        //    objFinHubAddOns.CreatedByUser = UserID;
        //    AddFinHubAddOns(objFinHubAddOns);
        //}

        //	throw new System.NotImplementedException("The method or operation is not implemented.");
        //}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetSearchItems implements the ISearchable Interface
        /// </summary>
        /// <param name="ModInfo">The ModuleInfo for the module to be Indexed</param>
        /// -----------------------------------------------------------------------------
        //public DotNetNuke.Services.Search.SearchItemInfoCollection GetSearchItems(DotNetNuke.Entities.Modules.ModuleInfo ModInfo)
        //{
        //SearchItemInfoCollection SearchItemCollection = new SearchItemInfoCollection();

        //List<FinHubAddOnsInfo> colFinHubAddOnss = GetFinHubAddOnss(ModInfo.ModuleID);

        //foreach (FinHubAddOnsInfo objFinHubAddOns in colFinHubAddOnss)
        //{
        //    SearchItemInfo SearchItem = new SearchItemInfo(ModInfo.ModuleTitle, objFinHubAddOns.Content, objFinHubAddOns.CreatedByUser, objFinHubAddOns.CreatedDate, ModInfo.ModuleID, objFinHubAddOns.ItemId.ToString(), objFinHubAddOns.Content, "ItemId=" + objFinHubAddOns.ItemId.ToString());
        //    SearchItemCollection.Add(SearchItem);
        //}

        //return SearchItemCollection;

        //	throw new System.NotImplementedException("The method or operation is not implemented.");
        //}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpgradeModule implements the IUpgradeable Interface
        /// </summary>
        /// <param name="Version">The current version of the module</param>
        /// -----------------------------------------------------------------------------
        //public string UpgradeModule(string Version)
        //{
        //	throw new System.NotImplementedException("The method or operation is not implemented.");
        //}

        #endregion
    }
}
