// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: Common.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using scg = global::System.Collections.Generic;
namespace ETModel {

  #region Messages
  public partial class C2R_Ping : pb::IMessage {
    private static readonly pb::MessageParser<C2R_Ping> _parser = new pb::MessageParser<C2R_Ping>(() => (C2R_Ping)MessagePool.Instance.Fetch(typeof(C2R_Ping)));
    public static pb::MessageParser<C2R_Ping> Parser { get { return _parser; } }

    private int rpcId_;
    public int RpcId {
      get { return rpcId_; }
      set {
        rpcId_ = value;
      }
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (RpcId != 0) {
        output.WriteRawTag(208, 5);
        output.WriteInt32(RpcId);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (RpcId != 0) {
        size += 2 + pb::CodedOutputStream.ComputeInt32Size(RpcId);
      }
      return size;
    }

    public void MergeFrom(pb::CodedInputStream input) {
      rpcId_ = 0;
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 720: {
            RpcId = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  public partial class R2C_Ping : pb::IMessage {
    private static readonly pb::MessageParser<R2C_Ping> _parser = new pb::MessageParser<R2C_Ping>(() => (R2C_Ping)MessagePool.Instance.Fetch(typeof(R2C_Ping)));
    public static pb::MessageParser<R2C_Ping> Parser { get { return _parser; } }

    private int rpcId_;
    public int RpcId {
      get { return rpcId_; }
      set {
        rpcId_ = value;
      }
    }

    private int error_;
    public int Error {
      get { return error_; }
      set {
        error_ = value;
      }
    }

    private string message_ = "";
    public string Message {
      get { return message_; }
      set {
        message_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (RpcId != 0) {
        output.WriteRawTag(208, 5);
        output.WriteInt32(RpcId);
      }
      if (Error != 0) {
        output.WriteRawTag(216, 5);
        output.WriteInt32(Error);
      }
      if (Message.Length != 0) {
        output.WriteRawTag(226, 5);
        output.WriteString(Message);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (RpcId != 0) {
        size += 2 + pb::CodedOutputStream.ComputeInt32Size(RpcId);
      }
      if (Error != 0) {
        size += 2 + pb::CodedOutputStream.ComputeInt32Size(Error);
      }
      if (Message.Length != 0) {
        size += 2 + pb::CodedOutputStream.ComputeStringSize(Message);
      }
      return size;
    }

    public void MergeFrom(pb::CodedInputStream input) {
      rpcId_ = 0;
      error_ = 0;
      message_ = "";
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 720: {
            RpcId = input.ReadInt32();
            break;
          }
          case 728: {
            Error = input.ReadInt32();
            break;
          }
          case 738: {
            Message = input.ReadString();
            break;
          }
        }
      }
    }

  }

  public partial class C2M_Reload : pb::IMessage {
    private static readonly pb::MessageParser<C2M_Reload> _parser = new pb::MessageParser<C2M_Reload>(() => (C2M_Reload)MessagePool.Instance.Fetch(typeof(C2M_Reload)));
    public static pb::MessageParser<C2M_Reload> Parser { get { return _parser; } }

    private int rpcId_;
    public int RpcId {
      get { return rpcId_; }
      set {
        rpcId_ = value;
      }
    }

    private string account_ = "";
    public string Account {
      get { return account_; }
      set {
        account_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    private string password_ = "";
    public string Password {
      get { return password_; }
      set {
        password_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (Account.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Account);
      }
      if (Password.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Password);
      }
      if (RpcId != 0) {
        output.WriteRawTag(208, 5);
        output.WriteInt32(RpcId);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (RpcId != 0) {
        size += 2 + pb::CodedOutputStream.ComputeInt32Size(RpcId);
      }
      if (Account.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Account);
      }
      if (Password.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Password);
      }
      return size;
    }

    public void MergeFrom(pb::CodedInputStream input) {
      account_ = "";
      password_ = "";
      rpcId_ = 0;
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 10: {
            Account = input.ReadString();
            break;
          }
          case 18: {
            Password = input.ReadString();
            break;
          }
          case 720: {
            RpcId = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  public partial class M2C_Reload : pb::IMessage {
    private static readonly pb::MessageParser<M2C_Reload> _parser = new pb::MessageParser<M2C_Reload>(() => (M2C_Reload)MessagePool.Instance.Fetch(typeof(M2C_Reload)));
    public static pb::MessageParser<M2C_Reload> Parser { get { return _parser; } }

    private int rpcId_;
    public int RpcId {
      get { return rpcId_; }
      set {
        rpcId_ = value;
      }
    }

    private int error_;
    public int Error {
      get { return error_; }
      set {
        error_ = value;
      }
    }

    private string message_ = "";
    public string Message {
      get { return message_; }
      set {
        message_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    public void WriteTo(pb::CodedOutputStream output) {
      if (RpcId != 0) {
        output.WriteRawTag(208, 5);
        output.WriteInt32(RpcId);
      }
      if (Error != 0) {
        output.WriteRawTag(216, 5);
        output.WriteInt32(Error);
      }
      if (Message.Length != 0) {
        output.WriteRawTag(226, 5);
        output.WriteString(Message);
      }
    }

    public int CalculateSize() {
      int size = 0;
      if (RpcId != 0) {
        size += 2 + pb::CodedOutputStream.ComputeInt32Size(RpcId);
      }
      if (Error != 0) {
        size += 2 + pb::CodedOutputStream.ComputeInt32Size(Error);
      }
      if (Message.Length != 0) {
        size += 2 + pb::CodedOutputStream.ComputeStringSize(Message);
      }
      return size;
    }

    public void MergeFrom(pb::CodedInputStream input) {
      rpcId_ = 0;
      error_ = 0;
      message_ = "";
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 720: {
            RpcId = input.ReadInt32();
            break;
          }
          case 728: {
            Error = input.ReadInt32();
            break;
          }
          case 738: {
            Message = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
