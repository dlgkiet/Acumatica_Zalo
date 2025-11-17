<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="ZL302000.aspx.cs" Inherits="Page_ZL302000" Title="Untitled Page" %>
<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
	<px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="AnNhienCafe.ZaloTemplateMaint"
        PrimaryView="Templates"
        >
		<CallbackCommands>
		</CallbackCommands>
	
		<DataTrees>
			<px:PXTreeDataMember TreeView="EntityItems" TreeKeys="Key" /></DataTrees></px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
	<px:PXFormView ID="PXFormView1" runat="server" DataSourceID="ds" DataMember="Templates" 
  Width="100%" DefaultControlID="ednotificationID">
  <Template>
    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" ></px:PXLayoutRule>
    <px:PXSelector runat="server" ID="PXSelector1" DataField="NotificationID" FilterByAllFields="True" AutoRefresh="True" TextField="Description" NullText="<NEW>" DataSourceID="ds">
      <GridProperties>
        <Columns>
          <px:PXGridColumn DataField="NotificationID" Width="60px"  ></px:PXGridColumn>
          <px:PXGridColumn DataField="Description" Width="120px"></px:PXGridColumn>
          <px:PXGridColumn DataField="Subject" Width="220px"></px:PXGridColumn>
          <px:PXGridColumn DataField="Screen" Width="60px"></px:PXGridColumn>
        </Columns>
      </GridProperties>
    </px:PXSelector>
  <px:PXTextEdit ID="edName" runat="server" DataField="Description" AlreadyLocalized="False" DefaultLocale=""  ></px:PXTextEdit>
    <px:PXSelector ID="edScreenID" runat="server" DataField="Screen" DisplayMode="Hint" FilterByAllFields="true" CommitChanges="True"
      AllowEdit="True" 
      EditImages-Normal="control@WebN" EditImages-Disabled="control@WebD"
      EditImages-Hover="control@WebH" EditImages-Pushed="control@WebP"></px:PXSelector>
    <px:PXSelector ID="edFrom" runat="server" DataField="From"
    FilterByAllFields="True" DisplayMode="Text" TextMode="Search" CommitChanges="True" ></px:PXSelector>
    <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L"></px:PXLayoutRule>
    <px:PXTreeSelector ID="edRefNoteId" runat="server" DataField="LinkToEntity"
      TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0"
      ShowRootNode="False" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true"
      AppendSelectedValue="False" AutoRefresh="true" TreeDataMember="EntityItemsWithPrevious" >
      <DataBindings>
        <px:PXTreeItemBinding DataMember="EntityItemsWithPrevious" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path"></px:PXTreeItemBinding>
      </DataBindings>
    </px:PXTreeSelector>
    <px:PXLayoutRule runat="server" Merge="true" LabelsWidth="S" ControlSize="L"></px:PXLayoutRule>
    <px:PXTreeSelector ID="edContact" runat="server" DataField="LinkToContact"
      TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0"
      ShowRootNode="False" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true"
      AppendSelectedValue="False" AutoRefresh="true" TreeDataMember="ContactItemsWithPrevious"  >
      <DataBindings>
        <px:PXTreeItemBinding DataMember="ContactItemsWithPrevious" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path"></px:PXTreeItemBinding>
      </DataBindings>
    </px:PXTreeSelector>
    <px:PXLayoutRule runat="server" Merge="true" LabelsWidth="S" ControlSize="L"></px:PXLayoutRule>
    <px:PXTreeSelector ID="edBAccount" runat="server" DataField="LinkToAccount"
      TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0"
      ShowRootNode="False" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true"
      AppendSelectedValue="False" AutoRefresh="true" TreeDataMember="AccountItemsWithPrevious">
      <DataBindings>
        <px:PXTreeItemBinding DataMember="AccountItemsWithPrevious" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path"></px:PXTreeItemBinding>
      </DataBindings>
    </px:PXTreeSelector>
    <px:PXLayoutRule runat="server" Merge="true" LabelsWidth="S" ControlSize="L"></px:PXLayoutRule>
    <px:PXDropDown ID="edActivityType" runat="server" DataField="ActivityType" />
    <%--<px:PXCheckBox SuppressLabel="True" ID="chkShowSendByEventsTabExpr" runat="server" DataField="ShowSendByEventsTabExpr"></px:PXCheckBox>
    <px:PXCheckBox SuppressLabel="True" ID="chkShowSendBySchedulesTabExpr" runat="server" DataField="ShowSendBySchedulesTabExpr" Visible="false" ></px:PXCheckBox>--%>
    <px:PXLayoutRule runat="server" StartRow="true" LabelsWidth="S" ControlSize="L" ></px:PXLayoutRule>
    <px:PXSelector ID="edNTo" runat="server" DataField="To"
    FilterByAllFields="True" DisplayMode="Text" TextMode="Search" CommitChanges="True" />
    <px:PXSelector ID="edCc" runat="server" DataField="Cc"
    FilterByAllFields="True" DisplayMode="Text" TextMode="Search" CommitChanges="True" />
    <px:PXSelector ID="edNBcc" runat="server" DataField="Bcc"
    FilterByAllFields="True" DisplayMode="Text" TextMode="Search" CommitChanges="True" />
    <px:PXTreeSelector ID="edsubject" runat="server" DataField="Subject" 
      TreeDataSourceID="ds" PopulateOnDemand="True" InitialExpandLevel="0"
      ShowRootNode="False" MinDropWidth="468" MaxDropWidth="600" AllowEditValue="true"
      AppendSelectedValue="True" AutoRefresh="true" TreeDataMember="EntityItemsWithPrevious" Width="746px">
      <DataBindings>
        <px:PXTreeItemBinding DataMember="EntityItemsWithPrevious" TextField="Name" ValueField="Path" ImageUrlField="Icon" ToolTipField="Path" ></px:PXTreeItemBinding>
      </DataBindings>
    </px:PXTreeSelector>
   <px:PXSelector ID="edReferenceNbr" runat="server" DataField="ReferenceNbr" Width="746px" />
    <px:PXTextEdit ID="edPreviewMessage" runat="server" DataField="PreviewMessage" ReadOnly="true" TextMode="MultiLine" Width="746px" />

  </Template>
  </px:PXFormView>

</asp:Content>
<asp:Content ID="cont3" ContentPlaceHolderID="phG" Runat="Server">
	<px:PXTab ID="tab" runat="server" Height="150px" Style="z-index: 100" Width="100%"
  DataSourceID="ds" DataMember="CurrentNotification">
  <Activity HighlightColor="" SelectedColor="" Width="" Height=""></Activity>
  <Items>
    <px:PXTabItem Text="Message">
  <Template>
	<px:PXRichTextEdit runat="server" ID="edBody" EncodeInstructions="true" AllowLoadTemplate="true" AllowPlaceholders="true" AllowInsertParameter="true" AllowInsertPrevParameter="True" AllowAttached="true" AllowSearch="true" AllowMacros="true" AllowSourceMode="true" FileAttribute="embedded" DataField="Body" Style='width:100%;'>
		<AutoSize Enabled="True" MinHeight="216" ></AutoSize>
		<LoadTemplate TypeName="AnNhienCafe.ZaloTemplateMaint" ViewName="Templates" Size="M" DataMember="Templates" ValueField="NotificationID" TextField="Description" DataSourceID="ds" ></LoadTemplate>
		<InsertDatafield DataSourceID="ds" DataMember="EntityItems" ValueField="Path" TextField="Name" ImageField="Icon" ></InsertDatafield>
		<InsertDatafieldPrev DataSourceID="ds" DataMember="PreviousEntityItems" ValueField="Path" TextField="Name" ImageField="Icon" ></InsertDatafieldPrev></px:PXRichTextEdit></Template>
    </px:PXTabItem>
	<px:PXTabItem />
	<px:PXTabItem />
	<px:PXTabItem />
	<px:PXTabItem /></Items>
  <AutoSize Container="Window" Enabled="True" MinHeight="250" ></AutoSize>
  </px:PXTab>
</asp:Content>