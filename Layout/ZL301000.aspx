<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true" 
    ValidateRequest="false" CodeFile="ZL301000.aspx.cs" Inherits="Page_ZL301000" Title="Zalo Token Maintenance" %>
<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" Runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="AnNhienCafe.ZaloTokenMaint"
        PrimaryView="ZaloToken">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="CallZaloApi" CommitChanges="True" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" Runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" DataMember="ZaloToken" 
        Width="100%" AllowAutoHide="false">
        <Template>
            <!-- App Configuration Section -->
            <px:PXLayoutRule ID="PXLayoutRule1" runat="server" StartGroup="True" 
                GroupCaption="Zalo App Configuration" StartRow="True" LabelsWidth="150px" />
            
            <px:PXTextEdit ID="edAppID" runat="server" DataField="AppID" Size="XL" />
            <px:PXTextEdit ID="edAppSecret" runat="server" DataField="AppSecret" Size="XL" />
            <px:PXTextEdit ID="edRefreshToken" runat="server" DataField="RefreshToken" 
                TextMode="MultiLine" Size="XXL" Height="80px" />
            
            <!-- Token Information Section -->
            <px:PXLayoutRule ID="PXLayoutRule2" runat="server" StartGroup="True" 
                GroupCaption="Access Token Information" StartRow="True" LabelsWidth="150px" />
            
            <px:PXTextEdit ID="edAccessToken" runat="server" DataField="AccessToken" 
                TextMode="MultiLine" Size="XXL" Height="100px" />
            
            <px:PXLayoutRule runat="server" Merge="True" />
            <px:PXDateTimeEdit ID="edAccessTokenExpiredAt" runat="server" 
                DataField="AccessTokenExpiredAt" Size="L" />
            <px:PXCheckBox ID="edIsAccessTokenValid" runat="server" 
                DataField="IsAccessTokenValid" Size="S" />
            <px:PXLayoutRule runat="server" />
            
            <!-- System Fields -->
            <px:PXLayoutRule ID="PXLayoutRule3" runat="server" StartGroup="True" 
                GroupCaption="System Information" StartRow="True" LabelsWidth="150px" />
            
            <px:PXLabel ID="lblTokenID" runat="server" DataField="TokenID" />

        </Template>
        <AutoSize Container="Window" Enabled="True" MinHeight="500" />
    </px:PXFormView>
</asp:Content>