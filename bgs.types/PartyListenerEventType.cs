using System;

namespace bgs.types
{
	public enum PartyListenerEventType
	{
		ERROR_RAISED = 0,
		JOINED_PARTY = 1,
		LEFT_PARTY = 2,
		PRIVACY_CHANGED = 3,
		MEMBER_JOINED = 4,
		MEMBER_LEFT = 5,
		MEMBER_ROLE_CHANGED = 6,
		RECEIVED_INVITE_ADDED = 7,
		RECEIVED_INVITE_REMOVED = 8,
		PARTY_INVITE_SENT = 9,
		PARTY_INVITE_REMOVED = 10,
		INVITE_REQUEST_ADDED = 11,
		INVITE_REQUEST_REMOVED = 12,
		CHAT_MESSAGE_RECEIVED = 13,
		PARTY_ATTRIBUTE_CHANGED = 14,
		MEMBER_ATTRIBUTE_CHANGED = 15,
		OPERATION_CALLBACK = 16
	}
}
