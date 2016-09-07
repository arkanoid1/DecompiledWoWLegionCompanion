using System;
using System.IO;

namespace bnet.protocol.account
{
	public class CreateGameAccountRequest : IProtoBuf
	{
		public bool HasAccount;

		private AccountId _Account;

		public bool HasRegion;

		private uint _Region;

		public bool HasProgram;

		private uint _Program;

		public bool HasRealmPermissions;

		private uint _RealmPermissions;

		public AccountId Account
		{
			get
			{
				return this._Account;
			}
			set
			{
				this._Account = value;
				this.HasAccount = (value != null);
			}
		}

		public uint Region
		{
			get
			{
				return this._Region;
			}
			set
			{
				this._Region = value;
				this.HasRegion = true;
			}
		}

		public uint Program
		{
			get
			{
				return this._Program;
			}
			set
			{
				this._Program = value;
				this.HasProgram = true;
			}
		}

		public uint RealmPermissions
		{
			get
			{
				return this._RealmPermissions;
			}
			set
			{
				this._RealmPermissions = value;
				this.HasRealmPermissions = true;
			}
		}

		public bool IsInitialized
		{
			get
			{
				return true;
			}
		}

		public void Deserialize(Stream stream)
		{
			CreateGameAccountRequest.Deserialize(stream, this);
		}

		public static CreateGameAccountRequest Deserialize(Stream stream, CreateGameAccountRequest instance)
		{
			return CreateGameAccountRequest.Deserialize(stream, instance, -1L);
		}

		public static CreateGameAccountRequest DeserializeLengthDelimited(Stream stream)
		{
			CreateGameAccountRequest createGameAccountRequest = new CreateGameAccountRequest();
			CreateGameAccountRequest.DeserializeLengthDelimited(stream, createGameAccountRequest);
			return createGameAccountRequest;
		}

		public static CreateGameAccountRequest DeserializeLengthDelimited(Stream stream, CreateGameAccountRequest instance)
		{
			long num = (long)((ulong)ProtocolParser.ReadUInt32(stream));
			num += stream.get_Position();
			return CreateGameAccountRequest.Deserialize(stream, instance, num);
		}

		public static CreateGameAccountRequest Deserialize(Stream stream, CreateGameAccountRequest instance, long limit)
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			instance.RealmPermissions = 1u;
			while (limit < 0L || stream.get_Position() < limit)
			{
				int num = stream.ReadByte();
				if (num != -1)
				{
					int num2 = num;
					switch (num2)
					{
					case 29:
						instance.Program = binaryReader.ReadUInt32();
						continue;
					case 30:
					case 31:
					{
						IL_7A:
						if (num2 == 10)
						{
							if (instance.Account == null)
							{
								instance.Account = AccountId.DeserializeLengthDelimited(stream);
							}
							else
							{
								AccountId.DeserializeLengthDelimited(stream, instance.Account);
							}
							continue;
						}
						if (num2 == 16)
						{
							instance.Region = ProtocolParser.ReadUInt32(stream);
							continue;
						}
						Key key = ProtocolParser.ReadKey((byte)num, stream);
						uint field = key.Field;
						if (field != 0u)
						{
							ProtocolParser.SkipKey(stream, key);
							continue;
						}
						throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
					}
					case 32:
						instance.RealmPermissions = ProtocolParser.ReadUInt32(stream);
						continue;
					}
					goto IL_7A;
				}
				if (limit >= 0L)
				{
					throw new EndOfStreamException();
				}
				return instance;
			}
			if (stream.get_Position() == limit)
			{
				return instance;
			}
			throw new ProtocolBufferException("Read past max limit");
		}

		public void Serialize(Stream stream)
		{
			CreateGameAccountRequest.Serialize(stream, this);
		}

		public static void Serialize(Stream stream, CreateGameAccountRequest instance)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			if (instance.HasAccount)
			{
				stream.WriteByte(10);
				ProtocolParser.WriteUInt32(stream, instance.Account.GetSerializedSize());
				AccountId.Serialize(stream, instance.Account);
			}
			if (instance.HasRegion)
			{
				stream.WriteByte(16);
				ProtocolParser.WriteUInt32(stream, instance.Region);
			}
			if (instance.HasProgram)
			{
				stream.WriteByte(29);
				binaryWriter.Write(instance.Program);
			}
			if (instance.HasRealmPermissions)
			{
				stream.WriteByte(32);
				ProtocolParser.WriteUInt32(stream, instance.RealmPermissions);
			}
		}

		public uint GetSerializedSize()
		{
			uint num = 0u;
			if (this.HasAccount)
			{
				num += 1u;
				uint serializedSize = this.Account.GetSerializedSize();
				num += serializedSize + ProtocolParser.SizeOfUInt32(serializedSize);
			}
			if (this.HasRegion)
			{
				num += 1u;
				num += ProtocolParser.SizeOfUInt32(this.Region);
			}
			if (this.HasProgram)
			{
				num += 1u;
				num += 4u;
			}
			if (this.HasRealmPermissions)
			{
				num += 1u;
				num += ProtocolParser.SizeOfUInt32(this.RealmPermissions);
			}
			return num;
		}

		public void SetAccount(AccountId val)
		{
			this.Account = val;
		}

		public void SetRegion(uint val)
		{
			this.Region = val;
		}

		public void SetProgram(uint val)
		{
			this.Program = val;
		}

		public void SetRealmPermissions(uint val)
		{
			this.RealmPermissions = val;
		}

		public override int GetHashCode()
		{
			int num = base.GetType().GetHashCode();
			if (this.HasAccount)
			{
				num ^= this.Account.GetHashCode();
			}
			if (this.HasRegion)
			{
				num ^= this.Region.GetHashCode();
			}
			if (this.HasProgram)
			{
				num ^= this.Program.GetHashCode();
			}
			if (this.HasRealmPermissions)
			{
				num ^= this.RealmPermissions.GetHashCode();
			}
			return num;
		}

		public override bool Equals(object obj)
		{
			CreateGameAccountRequest createGameAccountRequest = obj as CreateGameAccountRequest;
			return createGameAccountRequest != null && this.HasAccount == createGameAccountRequest.HasAccount && (!this.HasAccount || this.Account.Equals(createGameAccountRequest.Account)) && this.HasRegion == createGameAccountRequest.HasRegion && (!this.HasRegion || this.Region.Equals(createGameAccountRequest.Region)) && this.HasProgram == createGameAccountRequest.HasProgram && (!this.HasProgram || this.Program.Equals(createGameAccountRequest.Program)) && this.HasRealmPermissions == createGameAccountRequest.HasRealmPermissions && (!this.HasRealmPermissions || this.RealmPermissions.Equals(createGameAccountRequest.RealmPermissions));
		}

		public static CreateGameAccountRequest ParseFrom(byte[] bs)
		{
			return ProtobufUtil.ParseFrom<CreateGameAccountRequest>(bs, 0, -1);
		}
	}
}
