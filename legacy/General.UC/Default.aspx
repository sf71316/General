<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="General.UC._Default" %>

<%@ Register src="UC/FileUploaderUC.ascx" tagname="FileUploaderUC" tagprefix="uc1" %>

<%@ Register assembly="General.CC" namespace="General.CC" tagprefix="cc" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <uc1:FileUploaderUC ID="FileUploaderUC1" runat="server" />
    </div>
    </form>
   
</body>
</html>
