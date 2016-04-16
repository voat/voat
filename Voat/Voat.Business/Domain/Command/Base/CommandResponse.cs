using Newtonsoft.Json;

namespace Voat.Domain.Command
{
    public enum Status
    {
        NotProcessed = 0,
        Success = 1,
        Denied = 2,
        Ignored = 4,
        Invalid = 8
    }

    //We are not moving to a full CQRS implementation at this time, so for now some commands need to return
    //data such as identity values and vote counts. It would be inefficient to implement an eventing structure
    //at this time since the data will be present at the time of a command execution.
    public class CommandResponse
    {
        public CommandResponse(Status status, string description, string systemDescription = "")
        {
            this.Status = status;
            this.Description = description;
            this.SystemDescription = systemDescription;
        }

        /// <summary>
        /// The friendly description to be used if information is displayed on the UI or to the user.
        /// </summary>
        public virtual string Description { get; protected set; }

        public Status Status { get; protected set; }

        public bool Successfull
        {
            get { return this.Status == Status.Success; }
        }

        /// <summary>
        /// The system systemDescription of command response status.
        /// </summary>
        [JsonIgnore]
        public virtual string SystemDescription { get; protected set; }

        public static CommandResponse<R> Denied<R>(R response, string description, string systemDescription = "")
        {
            return new CommandResponse<R>(response, Status.Denied, description, systemDescription);
        }

        public static CommandResponse Denied(string description, string systemDescription = "")
        {
            return new CommandResponse(Status.Denied, description, systemDescription);
        }

        public static CommandResponse<R> Ignored<R>(R response, string description, string systemDescription = "")
        {
            return new CommandResponse<R>(response, Status.Ignored, description, systemDescription);
        }

        public static CommandResponse Ignored(string description, string systemDescription = "")
        {
            return new CommandResponse(Status.Ignored, description, systemDescription);
        }

        public static CommandResponse<R> Success<R>(R response)
        {
            return new CommandResponse<R>(response, Status.Success, "", "");
        }

        public static CommandResponse Success()
        {
            return new CommandResponse(Status.Success, "", "");
        }
        public static CommandResponse<M> Map<T, M>(CommandResponse<T> response, M mapped)
        {
            return new CommandResponse<M>(mapped, response.Status, response.Description, response.SystemDescription);
        }
    }

    public class CommandResponse<R> : CommandResponse
    {
        public CommandResponse(R response, Status status, string description, string systemDescription = "") : base(status, description, systemDescription)
        {
            this.Response = response;
        }

        public R Response { get; set; }
    }
}
