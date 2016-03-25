<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SQL_Docs_Generator._Default" %>



<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

   
     <h1>SQL Server Documentation Generator</h1>
     <div class="row">       
      
       <div class="col-md-4">      
                <asp:Label ID="Label2" runat="server" Text="Server name" Class="userlabel"></asp:Label><br/>
                <asp:TextBox ID="txtServerName" Text="" runat="server" TextMode="SingleLine" Class="usertext"></asp:TextBox><br />
                <asp:Label ID="Label3" runat="server" Text="Authentication" Class="userlabel"></asp:Label><br/>
                <asp:DropDownList ID="drpAuthType" runat="server" OnTextChanged="drpAuthType_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>                
                <asp:Panel  runat ="server"  id="panel_SQL_auth" Visible="false">           
                    <asp:Label ID="Label4" runat="server" Text="Login" Class="userlabel"></asp:Label><br/>
                    <asp:TextBox ID="txtLogin" Text="" runat="server" TextMode="SingleLine"></asp:TextBox><br />
                    <asp:Label ID="Label5" runat="server" Text="Password" Class="userlabel"></asp:Label><br/>
                    <asp:TextBox ID="txtPassword" Text="" runat="server" TextMode="SingleLine"></asp:TextBox><br />
                </asp:Panel><br />                
       </div>
            
       <div class="col-md-4"> 
                <asp:Label ID="Label6" runat="server" Text="Database list" Class="userlabel"></asp:Label><br/>                
                <asp:DropDownList ID="drpDatabase" runat="server" OnSelectedIndexChanged="drpDatabase_SelectedIndexChanged" AutoPostBack="true" ></asp:DropDownList> <br />                                              
                <asp:Label ID="Label1" runat="server" Text="Options" Class="userlabel"></asp:Label><br/>  
                <asp:CheckBox ID="chkDescr" runat="server"  Checked="true" Text="with description"></asp:CheckBox><br />
                <asp:CheckBox ID="chkAuthor" runat="server"  Checked="true" Text="with author"></asp:CheckBox> <br />     
                <asp:CheckBox ID="chkCreated" runat="server"  Checked="true" Text="with created date"></asp:CheckBox><br />
                <asp:CheckBox ID="chkParams" runat="server"  Checked="true" Text="with parameters"></asp:CheckBox><br />
                <asp:CheckBox ID="chkStatuses" runat="server"  Checked="true" Text="with statuses"></asp:CheckBox><br />                                                      
       </div>
    </div>
     

    <div class="row">
         <div class="col-md-4">  
             <asp:Button ID="btnConnect" OnClick="btnConnect_Click" Text="Connect" runat="server"  Class="userbutton"/><br />
             </div>

        <div class="col-md-4">  
            <asp:Button ID="btnGenerate" OnClick="btnGenerate_Click" Text="Generate" runat="server" Class="userbutton" /><br />      
             </div>
    </div>

    <div class="row">      
         <asp:Label ID="lblError" runat="server" Text="" Class="usererrorlabel" ></asp:Label><br/>   
    </div>
    </asp:Content>
