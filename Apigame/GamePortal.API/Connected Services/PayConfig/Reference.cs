﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GamePortal.API.PayConfig {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CardConfig", Namespace="http://tempuri.org/")]
    [System.SerializableAttribute()]
    public partial class CardConfig : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private int IDField;
        
        private GamePortal.API.PayConfig.Telco TypeField;
        
        private int PrizeField;
        
        private bool EnableField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PayOrderConfigField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string PartnerConfigField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public int ID {
            get {
                return this.IDField;
            }
            set {
                if ((this.IDField.Equals(value) != true)) {
                    this.IDField = value;
                    this.RaisePropertyChanged("ID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public GamePortal.API.PayConfig.Telco Type {
            get {
                return this.TypeField;
            }
            set {
                if ((this.TypeField.Equals(value) != true)) {
                    this.TypeField = value;
                    this.RaisePropertyChanged("Type");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=2)]
        public int Prize {
            get {
                return this.PrizeField;
            }
            set {
                if ((this.PrizeField.Equals(value) != true)) {
                    this.PrizeField = value;
                    this.RaisePropertyChanged("Prize");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true, Order=3)]
        public bool Enable {
            get {
                return this.EnableField;
            }
            set {
                if ((this.EnableField.Equals(value) != true)) {
                    this.EnableField = value;
                    this.RaisePropertyChanged("Enable");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=4)]
        public string PayOrderConfig {
            get {
                return this.PayOrderConfigField;
            }
            set {
                if ((object.ReferenceEquals(this.PayOrderConfigField, value) != true)) {
                    this.PayOrderConfigField = value;
                    this.RaisePropertyChanged("PayOrderConfig");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=5)]
        public string PartnerConfig {
            get {
                return this.PartnerConfigField;
            }
            set {
                if ((object.ReferenceEquals(this.PartnerConfigField, value) != true)) {
                    this.PartnerConfigField = value;
                    this.RaisePropertyChanged("PartnerConfig");
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
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Telco", Namespace="http://tempuri.org/")]
    public enum Telco : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        VTT = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        VMS = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        VNP = 2,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="PayConfig.configSoap")]
    public interface configSoap {
        
        // CODEGEN: Generating message contract since element name GetCardConfigsResult from namespace http://tempuri.org/ is not marked nillable
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCardConfigs", ReplyAction="*")]
        GamePortal.API.PayConfig.GetCardConfigsResponse GetCardConfigs(GamePortal.API.PayConfig.GetCardConfigsRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/GetCardConfigs", ReplyAction="*")]
        System.Threading.Tasks.Task<GamePortal.API.PayConfig.GetCardConfigsResponse> GetCardConfigsAsync(GamePortal.API.PayConfig.GetCardConfigsRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetCardConfigsRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetCardConfigs", Namespace="http://tempuri.org/", Order=0)]
        public GamePortal.API.PayConfig.GetCardConfigsRequestBody Body;
        
        public GetCardConfigsRequest() {
        }
        
        public GetCardConfigsRequest(GamePortal.API.PayConfig.GetCardConfigsRequestBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute()]
    public partial class GetCardConfigsRequestBody {
        
        public GetCardConfigsRequestBody() {
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class GetCardConfigsResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Name="GetCardConfigsResponse", Namespace="http://tempuri.org/", Order=0)]
        public GamePortal.API.PayConfig.GetCardConfigsResponseBody Body;
        
        public GetCardConfigsResponse() {
        }
        
        public GetCardConfigsResponse(GamePortal.API.PayConfig.GetCardConfigsResponseBody Body) {
            this.Body = Body;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.Runtime.Serialization.DataContractAttribute(Namespace="http://tempuri.org/")]
    public partial class GetCardConfigsResponseBody {
        
        [System.Runtime.Serialization.DataMemberAttribute(EmitDefaultValue=false, Order=0)]
        public GamePortal.API.PayConfig.CardConfig[] GetCardConfigsResult;
        
        public GetCardConfigsResponseBody() {
        }
        
        public GetCardConfigsResponseBody(GamePortal.API.PayConfig.CardConfig[] GetCardConfigsResult) {
            this.GetCardConfigsResult = GetCardConfigsResult;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface configSoapChannel : GamePortal.API.PayConfig.configSoap, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class configSoapClient : System.ServiceModel.ClientBase<GamePortal.API.PayConfig.configSoap>, GamePortal.API.PayConfig.configSoap {
        
        public configSoapClient() {
        }
        
        public configSoapClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public configSoapClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public configSoapClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public configSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        GamePortal.API.PayConfig.GetCardConfigsResponse GamePortal.API.PayConfig.configSoap.GetCardConfigs(GamePortal.API.PayConfig.GetCardConfigsRequest request) {
            return base.Channel.GetCardConfigs(request);
        }
        
        public GamePortal.API.PayConfig.CardConfig[] GetCardConfigs() {
            GamePortal.API.PayConfig.GetCardConfigsRequest inValue = new GamePortal.API.PayConfig.GetCardConfigsRequest();
            inValue.Body = new GamePortal.API.PayConfig.GetCardConfigsRequestBody();
            GamePortal.API.PayConfig.GetCardConfigsResponse retVal = ((GamePortal.API.PayConfig.configSoap)(this)).GetCardConfigs(inValue);
            return retVal.Body.GetCardConfigsResult;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<GamePortal.API.PayConfig.GetCardConfigsResponse> GamePortal.API.PayConfig.configSoap.GetCardConfigsAsync(GamePortal.API.PayConfig.GetCardConfigsRequest request) {
            return base.Channel.GetCardConfigsAsync(request);
        }
        
        public System.Threading.Tasks.Task<GamePortal.API.PayConfig.GetCardConfigsResponse> GetCardConfigsAsync() {
            GamePortal.API.PayConfig.GetCardConfigsRequest inValue = new GamePortal.API.PayConfig.GetCardConfigsRequest();
            inValue.Body = new GamePortal.API.PayConfig.GetCardConfigsRequestBody();
            return ((GamePortal.API.PayConfig.configSoap)(this)).GetCardConfigsAsync(inValue);
        }
    }
}