using System;
using System.Collections.Generic;


//
namespace Icheon
{
    public partial class Domino : TcpClientParent
    {
        public enum ErrorCode
        {
            NoError = 0,
            NoDocumentLoaded,
            WrongNumberOfParameters,
            ObjectNotFound,
            CommandUnknown,
            WrongObjectType,
            WrongParameters,
            TransactionFailed,
            CounterNotFound,
            FileIOError,
            CommandTimeout,
            NoMessageMustBeOpen,
            SourceNotFound,
            FunctionNotSupported,
            InternalFault,
            InvalidXML,
            TransactionLocked,
            NoTransactionOpen,
            VariableNotExist,
            CommandParseError,
            BufferDataIndexInUse,
            ObjectAlreadyExists,
            ObjectCreationFailed,
            OperationNotAllowed,
            NoConnectionToDSP,
            NoPermission,
            RemoteBufferingNotActive,
            ColdStartFailed,
            VectorCompilationFailed,
            InternalLoginError,
            MultiPrinterNameNotFound,
            MultiPrinterResponseMismatch,
            NetworkSettingFailed,
            MarkingEngineRunning
        }

        //
        public static readonly Dictionary<ErrorCode, string> ErrorMessages = new Dictionary<ErrorCode, string>
        {
            { ErrorCode.NoError, "No error occurred" },
            { ErrorCode.NoDocumentLoaded, "No document loaded" },
            { ErrorCode.WrongNumberOfParameters, "Wrong number of parameters" },
            { ErrorCode.ObjectNotFound, "Object with specified name not found" },
            { ErrorCode.CommandUnknown, "Command unknown" },
            { ErrorCode.WrongObjectType, "Wrong Object-Type" },
            { ErrorCode.WrongParameters, "Wrong Parameters" },
            { ErrorCode.TransactionFailed, "Transaction failed" },
            { ErrorCode.CounterNotFound, "Specified Counter not found" },
            { ErrorCode.FileIOError, "Error while File-I/O" },
            { ErrorCode.CommandTimeout, "Timeout for a command that required a response" },
            { ErrorCode.NoMessageMustBeOpen, "No message must be open" },
            { ErrorCode.SourceNotFound, "Source not found" },
            { ErrorCode.FunctionNotSupported, "The function is not supported in this system configuration" },
            { ErrorCode.InternalFault, "Internal fault" },
            { ErrorCode.InvalidXML, "The XML code that was sent is not valid" },
            { ErrorCode.TransactionLocked, "Transaction is locked" },
            { ErrorCode.NoTransactionOpen, "No transaction is open" },
            { ErrorCode.VariableNotExist, "The variable does not exist in the current label" },
            { ErrorCode.CommandParseError, "Command parse error" },
            { ErrorCode.BufferDataIndexInUse, "The index specified in the BUFFERDATA command is already in use or more than 9999 data records have been preloaded." },
            { ErrorCode.ObjectAlreadyExists, "Object cannot be added with OBJECTADD as the specified object name already exists." },
            { ErrorCode.ObjectCreationFailed, "Object creation failed (e.g., the specified object type is not supported)." },
            { ErrorCode.OperationNotAllowed, "Operation not allowed." },
            { ErrorCode.NoConnectionToDSP, "No connection to DSP card." },
            { ErrorCode.NoPermission, "No permissions to perform this action." },
            { ErrorCode.RemoteBufferingNotActive, "Remote data buffering is not active." },
            { ErrorCode.ColdStartFailed, "Cold start procedure failed (laser warm-up incomplete)." },
            { ErrorCode.VectorCompilationFailed, "Vector compilation failed (e.g., objects out of bounds or wrong encoder direction)." },
            { ErrorCode.InternalLoginError, "Internal Error occurring while logging in the communication interface user – please contact Domino." },
            { ErrorCode.MultiPrinterNameNotFound, "Multi Printer configuration – The given printer name in a command doesn’t exist." },
            { ErrorCode.MultiPrinterResponseMismatch, "Multi Printer configuration – The response of a controller doesn’t match to the command." },
            { ErrorCode.NetworkSettingFailed, "Setting or getting network address or DNS is not successful." },
            { ErrorCode.MarkingEngineRunning, "Current command failed because marking engine is still running. It can only work after the marking engine stops." }
        };

        //
        public static string GetErrorMessage(int errorCode)
        {
            if (Enum.IsDefined(typeof(ErrorCode), errorCode))
                return ErrorMessages[(ErrorCode)errorCode];
            return $"[{errorCode}] Unknown error code.";
        }
    }// public partial class Domino : TcpClientParent
}// namespace Icheon