﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3603
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ErrorReport", Namespace="http://tempuri.org/")]
    [System.SerializableAttribute()]
    public partial class ErrorReport : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ProductNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ProductVersionField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string UserActivityField;
        
        private System.DateTime DateOccurredField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReferencedAssembly[] AssembliesField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ExceptionReport[] ExceptionsField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string ProductName {
            get {
                return this.ProductNameField;
            }
            set {
                if ((object.ReferenceEquals(this.ProductNameField, value) != true)) {
                    this.ProductNameField = value;
                    this.RaisePropertyChanged("ProductName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string ProductVersion {
            get {
                return this.ProductVersionField;
            }
            set {
                if ((object.ReferenceEquals(this.ProductVersionField, value) != true)) {
                    this.ProductVersionField = value;
                    this.RaisePropertyChanged("ProductVersion");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string UserActivity {
            get {
                return this.UserActivityField;
            }
            set {
                if ((object.ReferenceEquals(this.UserActivityField, value) != true)) {
                    this.UserActivityField = value;
                    this.RaisePropertyChanged("UserActivity");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=3)]
        public System.DateTime DateOccurred {
            get {
                return this.DateOccurredField;
            }
            set {
                if ((this.DateOccurredField.Equals(value) != true)) {
                    this.DateOccurredField = value;
                    this.RaisePropertyChanged("DateOccurred");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=4)]
        public TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReferencedAssembly[] Assemblies {
            get {
                return this.AssembliesField;
            }
            set {
                if ((object.ReferenceEquals(this.AssembliesField, value) != true)) {
                    this.AssembliesField = value;
                    this.RaisePropertyChanged("Assemblies");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=5)]
        public TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ExceptionReport[] Exceptions {
            get {
                return this.ExceptionsField;
            }
            set {
                if ((object.ReferenceEquals(this.ExceptionsField, value) != true)) {
                    this.ExceptionsField = value;
                    this.RaisePropertyChanged("Exceptions");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ReferencedAssembly", Namespace="http://tempuri.org/")]
    [System.SerializableAttribute()]
    public partial class ReferencedAssembly : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string NameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FileVersionField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string Name {
            get {
                return this.NameField;
            }
            set {
                if ((object.ReferenceEquals(this.NameField, value) != true)) {
                    this.NameField = value;
                    this.RaisePropertyChanged("Name");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=1)]
        public string FileVersion {
            get {
                return this.FileVersionField;
            }
            set {
                if ((object.ReferenceEquals(this.FileVersionField, value) != true)) {
                    this.FileVersionField = value;
                    this.RaisePropertyChanged("FileVersion");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ExceptionReport", Namespace="http://tempuri.org/")]
    [System.SerializableAttribute()]
    public partial class ExceptionReport : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ExceptionTypeField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string MessageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string StackTraceField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DataField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string ExceptionType {
            get {
                return this.ExceptionTypeField;
            }
            set {
                if ((object.ReferenceEquals(this.ExceptionTypeField, value) != true)) {
                    this.ExceptionTypeField = value;
                    this.RaisePropertyChanged("ExceptionType");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string Message {
            get {
                return this.MessageField;
            }
            set {
                if ((object.ReferenceEquals(this.MessageField, value) != true)) {
                    this.MessageField = value;
                    this.RaisePropertyChanged("Message");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false)]
        public string StackTrace {
            get {
                return this.StackTraceField;
            }
            set {
                if ((object.ReferenceEquals(this.StackTraceField, value) != true)) {
                    this.StackTraceField = value;
                    this.RaisePropertyChanged("StackTrace");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=3)]
        public string Data {
            get {
                return this.DataField;
            }
            set {
                if ((object.ReferenceEquals(this.DataField, value) != true)) {
                    this.DataField = value;
                    this.RaisePropertyChanged("Data");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="BoxSoftwareServices.ErrorReportingSoap")]
    public interface ErrorReportingSoap {
        
        // CODEGEN: Generating message contract since element name content from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ReportError", ReplyAction="*")]
        TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorResponse ReportError(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorRequest request);
        
        // CODEGEN: Generating message contract since element name report from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ReportAnError", ReplyAction="*")]
        TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorResponse ReportAnError(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ReportErrorRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ReportError", Namespace="http://tempuri.org/", Order=0)]
        public TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorRequestBody Body;
        
        public ReportErrorRequest() {
        }
        
        public ReportErrorRequest(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class ReportErrorRequestBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public string content;
        
        public ReportErrorRequestBody() {
        }
        
        public ReportErrorRequestBody(string content) {
            this.content = content;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ReportErrorResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ReportErrorResponse", Namespace="http://tempuri.org/", Order=0)]
        public TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorResponseBody Body;
        
        public ReportErrorResponse() {
        }
        
        public ReportErrorResponse(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute()]
    public partial class ReportErrorResponseBody {
        
        public ReportErrorResponseBody() {
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ReportAnErrorRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ReportAnError", Namespace="http://tempuri.org/", Order=0)]
        public TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorRequestBody Body;
        
        public ReportAnErrorRequest() {
        }
        
        public ReportAnErrorRequest(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class ReportAnErrorRequestBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReport report;
        
        public ReportAnErrorRequestBody() {
        }
        
        public ReportAnErrorRequestBody(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReport report) {
            this.report = report;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class ReportAnErrorResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="ReportAnErrorResponse", Namespace="http://tempuri.org/", Order=0)]
        public TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorResponseBody Body;
        
        public ReportAnErrorResponse() {
        }
        
        public ReportAnErrorResponse(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute()]
    public partial class ReportAnErrorResponseBody {
        
        public ReportAnErrorResponseBody() {
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface ErrorReportingSoapChannel : TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReportingSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class ErrorReportingSoapClient : System.ServiceModel.ClientBase<TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReportingSoap>, TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReportingSoap {
        
        public ErrorReportingSoapClient() {
        }
        
        public ErrorReportingSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public ErrorReportingSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ErrorReportingSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public ErrorReportingSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorResponse TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReportingSoap.ReportError(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorRequest request) {
            return base.Channel.ReportError(request);
        }
        
        public void ReportError(string content) {
            TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorRequest inValue = new TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorRequest();
            inValue.Body = new TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorRequestBody();
            inValue.Body.content = content;
            TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportErrorResponse retVal = ((TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReportingSoap)(this)).ReportError(inValue);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorResponse TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReportingSoap.ReportAnError(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorRequest request) {
            return base.Channel.ReportAnError(request);
        }
        
        public void ReportAnError(TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReport report) {
            TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorRequest inValue = new TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorRequest();
            inValue.Body = new TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorRequestBody();
            inValue.Body.report = report;
            TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ReportAnErrorResponse retVal = ((TheBoxSoftware.DeveloperSuite.LiveDocumenter.BoxSoftwareServices.ErrorReportingSoap)(this)).ReportAnError(inValue);
        }
    }
}