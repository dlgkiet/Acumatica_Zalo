<%@ Page Language="C#" MasterPageFile="~/MasterPages/FormView.master" AutoEventWireup="true"
    ValidateRequest="false" CodeFile="ZL302000.aspx.cs" Inherits="Page_ZL302000"
    Title="Zalo Message Templates" %>

<%@ MasterType VirtualPath="~/MasterPages/FormView.master" %>

<asp:Content ID="cont1" ContentPlaceHolderID="phDS" runat="Server">
    <px:PXDataSource ID="ds" runat="server" Visible="True" Width="100%"
        TypeName="ANCafe.ZaloTemplateMaint"
        PrimaryView="Templates">
        <CallbackCommands>
            <px:PXDSCallbackCommand Name="insertMergeField" />
        </CallbackCommands>
    </px:PXDataSource>
</asp:Content>

<asp:Content ID="cont2" ContentPlaceHolderID="phF" runat="Server">
    <px:PXFormView ID="form" runat="server" DataSourceID="ds" Style="z-index: 100"
        Width="100%" DataMember="Templates" TabIndex="100">
        <Template>
            <!-- Left Column -->
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXLayoutRule runat="server" GroupCaption="Template Information" StartGroup="True" />
            <px:PXNumberEdit ID="edNotificationID" runat="server" DataField="NotificationID" />
            <px:PXTextEdit ID="edDescription" runat="server" DataField="Description" />
            <px:PXCheckBox ID="chkIsActive" runat="server" DataField="IsActive" CommitChanges="True" />
            <px:PXSelector ID="edScreen" runat="server" DataField="Screen" CommitChanges="True" />
            <px:PXDropDown ID="edActivityType" runat="server" DataField="ActivityType" CommitChanges="True" />

            <!-- Right Column -->
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="M" />
            <px:PXLayoutRule runat="server" GroupCaption="Recipient Information" StartGroup="True" />
            <px:PXTextEdit ID="edSubscriberID" runat="server" DataField="SubscriberID" />
            <px:PXTextEdit ID="edFrom" runat="server" DataField="From" />
            <px:PXTextEdit ID="edTo" runat="server" DataField="To" />
            <px:PXTextEdit ID="edCc" runat="server" DataField="Cc" />
            <px:PXTextEdit ID="edBcc" runat="server" DataField="Bcc" />

            <!-- Full Width Section -->
            <px:PXLayoutRule runat="server" StartColumn="True" LabelsWidth="SM" ControlSize="XL" />
            <px:PXLayoutRule runat="server" GroupCaption="Message Content" StartGroup="True" />
            <px:PXTextEdit ID="edSubject" runat="server" DataField="Subject" />

            <!-- Links Section -->
            <px:PXLayoutRule runat="server" GroupCaption="Reference Information" StartGroup="True" />
            <px:PXTextEdit ID="edLinkToEntity" runat="server" DataField="LinkToEntity" />
            <px:PXTextEdit ID="edLinkToContact" runat="server" DataField="LinkToContact" />
            <px:PXTextEdit ID="edLinkToAccount" runat="server" DataField="LinkToAccount" />
            <px:PXLayoutRule runat="server" GroupCaption="Reference Information" StartGroup="True" />
            <px:PXSelector ID="edReferenceNumber" runat="server"  DataField="ReferenceNbr" CommitChanges="True" AutoRefresh="True" />

            <!-- Message Body Section -->
            <px:PXLayoutRule runat="server" StartRow="True" />
            <px:PXLayoutRule runat="server" GroupCaption="Message Body" StartGroup="True" />
            <px:PXTextEdit ID="edBody" runat="server" DataField="Body" TextMode="MultiLine" 
                Height="200px" Width="100%" Style="font-family: 'Courier New', monospace;" />

            <!-- Action Buttons -->
            <px:PXLayoutRule runat="server" StartRow="True" />
            <px:PXPanel ID="pnlActions" runat="server" RenderStyle="Simple" Style="margin: 10px 0;">
                <px:PXButton ID="btnInsertField" runat="server" Text="Insert Merge Field" CommandName="insertMergeField" 
                    CommandSourceID="ds" Style="margin-right: 5px;" />
            </px:PXPanel>

            <!-- Preview Section -->
            <px:PXLayoutRule runat="server" StartRow="True" />
            <px:PXLayoutRule runat="server" GroupCaption="Message Preview" StartGroup="True" />
            <px:PXTextEdit ID="edPreviewMessage" runat="server" DataField="PreviewMessage" 
                TextMode="MultiLine" Height="150px" Width="100%" 
                Style="font-family: 'Courier New', monospace; background-color: #f9f9f9;" />

        </Template>
    </px:PXFormView>

    <!-- Grid Template List -->
    <px:PXGrid ID="grid" runat="server" DataSourceID="ds" Style="z-index: 100; margin-top: 20px;"
        Width="100%" Height="300px" SkinID="Details" TabIndex="300" FilesIndicator="False" 
        NoteIndicator="False">
        <Levels>
            <px:PXGridLevel DataMember="Templates">
                <Columns>
                    <px:PXGridColumn DataField="NotificationID" Width="70" />
                    <px:PXGridColumn DataField="Description" Width="200" />
                    <px:PXGridColumn DataField="IsActive" Width="60" Type="CheckBox" />
                    <px:PXGridColumn DataField="Screen" Width="150" />
                    <px:PXGridColumn DataField="ActivityType" Width="100" />
                    <px:PXGridColumn DataField="Subject" Width="200" />
                    <px:PXGridColumn DataField="CreatedDateTime" Width="130" />
                    <px:PXGridColumn DataField="LastModifiedDateTime" Width="130" />
                </Columns>
            </px:PXGridLevel>
        </Levels>
        <AutoSize Enabled="True" MinHeight="150" />
        <Mode AllowAddNew="True" AllowDelete="True" AllowUpdate="True" />
        <ActionBar>
            <Actions>
                <AddNew Enabled="True" />
                <Delete Enabled="True" />
            </Actions>
        </ActionBar>
    </px:PXGrid>
</asp:Content>