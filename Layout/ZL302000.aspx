<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormTab.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="ZL302000.aspx.cs" Inherits="Page_ZL302000"
    Title="Zalo Message Templates" %>

<%@ MasterType VirtualPath="~/MasterPages/FormTab.master" %>
<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="AnNhienCafe.ZaloTemplateMaint"
        PrimaryView="Templates">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="showPreview" />
            <px:PXDSCallbackCommand Name="sendZaloMessage" />
            <px:PXDSCallbackCommand Name="LoadFields" Visible="False" />
        </CallbackCommands>

        <DataTrees>
            <px:PXTreeDataMember TreeView="EntityItems" TreeKeys="Key" />
        </DataTrees>

    </px:PXDataSource>
</asp:Content>
<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="Templates"
        Width="100%" DefaultControlID="edNotificationID">
        <Template>
            <!-- Cột 1 -->
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
            <px:PXNumberEdit ID="edNotificationID" runat="server" DataField="NotificationID" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" CommitChanges="true" />
            <px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive" />
            <px:PXSelector ID="edScreen" runat="server" DataField="Screen" />
            <px:PXSelector ID="edFrom" runat="server" DataField="From" AutoRefresh="True" CommitChanges="True" DisplayMode="Text" TextMode="Search" />

            <!-- Cột 2 -->
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
            <px:PXSelector ID="edTo" runat="server" DataField="To" AutoRefresh="True" CommitChanges="True" DisplayMode="Text" TextMode="Search" />
            <px:PXSelector ID="edCc" runat="server" DataField="Cc" AutoRefresh="True" CommitChanges="True" DisplayMode="Text" TextMode="Search" />
            <px:PXSelector ID="edBcc" runat="server" DataField="Bcc" AutoRefresh="True" CommitChanges="True" DisplayMode="Text" TextMode="Search" />
            <px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />
            <px:PXDropDown ID="edActivityType" runat="server" DataField="ActivityType" />

            <!-- Liên kết -->
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="S" ControlSize="L" />
            <px:PXTextEdit ID="edLinkToEntity" runat="server" DataField="LinkToEntity" />
            <px:PXTextEdit ID="edLinkToContact" runat="server" DataField="LinkToContact" />
            <px:PXTextEdit ID="edLinkToAccount" runat="server" DataField="LinkToAccount" />
            <px:PXSelector ID="edReferenceNbr" runat="server" DataField="ReferenceNbr" CommitChanges="True" />

            <!-- Preview Message -->
            <px:PXLayoutRule runat="server" StartRow="True" LabelsWidth="M" ControlSize="XXL" />
            <px:PXTextEdit ID="edPreviewMessage" runat="server" DataField="PreviewMessage" TextMode="MultiLine" Height="200px" Width="100%" />
        </Template>
    </px:PXFormView>

    <!-- Tab Giống Email Template -->
    <px:PXTab ID="tabMessage" runat="server" DataSourceID="ds" Width="100%" Height="500px" DataMember="Templates">
        <Items>
            <px:PXTabItem Text="Message">
                <Template>
                    <px:PXRichTextEdit ID="edBody" runat="server" DataField="Body" AllowInsertParameter="True" AllowSourceMode="True"
                        AllowSearch="True" AllowPlaceholders="True" AllowMacros="True" AllowImages="True" AllowTables="True"
                        Style="width: 100%" CommitChanges="True">
                        <AutoSize Enabled="True" MinHeight="400" />
                        <InsertDatafield DataSourceID="ds" DataMember="EntityItems" TextField="Name" ValueField="Path" ImageField="Icon" />
                    </px:PXRichTextEdit>
                </Template>
            </px:PXTabItem>
        </Items>
    </px:PXTab>
</asp:Content>