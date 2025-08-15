<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormDetail.master" AutoEventWireup="true" ValidateRequest="false" CodeFile="ZL303000.aspx.cs" Inherits="Page_ZL303000" Title="Zalo User Management" %>

<%@ MasterType VirtualPath="~/MasterPages/FormDetail.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="AnNhienCafe.ZaloUserMaint"
        PrimaryView="ZaloUsers">
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont3" ContentPlaceHolderID="phG" runat="Server">
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Width="100%" Height="300px" SkinID="Details" AllowAutoHide="false">
        <Levels>
            <px:PXGridLevel DataMember="ZaloUsers">
                <Columns>
                    <px:PXGridColumn DataField="ZaloUserID" Width="150px" />
                    <px:PXGridColumn DataField="Name" Width="200px" />
                    <px:PXGridColumn DataField="Role" Width="100px" />
                    <px:PXGridColumn DataField="CreatedDate" Width="140px" />
                    <px:PXGridColumn DataField="IsActive" Width="80px" Type="CheckBox" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Container="Window" Enabled="True" MinHeight="300" />
    </px:PXGrid>
</asp:Content>
