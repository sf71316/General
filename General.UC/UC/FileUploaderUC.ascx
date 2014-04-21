<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileUploaderUC.ascx.cs"
    Inherits="General.UC.FileUploaderUC" %>



 
<%@ Register assembly="General.CC" namespace="General.CC" tagprefix="sf" %>








 


 
<sf:Uploadify ID="Uploadify1" runat="server" 
    UploadHandler="~/GeneralUploader.axd" ButtonText="瀏覽檔案" 
    Extension="*.jpg;*.png;*.gif" MultiUpload="True" UploadButtonText="上傳檔案" />









 


 
