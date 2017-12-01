using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;
using Voat.Data.Models;
using System.Linq;
using System.Threading.Tasks;
using Voat.Utilities;
using System.Text.RegularExpressions;

namespace Voat.Data
{
    public partial class Repository
    {
        public bool IsApiKeyValid(string apiPublicKey)
        {
            var key = GetApiKey(apiPublicKey);

            if (key != null && key.IsActive)
            {
                //TODO: This needs to be non-blocking and non-queued. If 20 threads with same apikey are accessing this method at once we don't want to perform 20 updates on record.
                //keep track of last access date
                key.LastAccessDate = CurrentDate;
                _db.SaveChanges();

                return true;
            }

            return false;
        }

        public ApiClient GetApiKey(string apiPublicKey)
        {
            var result = (from x in this._db.ApiClient
                          where x.PublicKey == apiPublicKey
                          select x).FirstOrDefault();
            return result;
        }

        [Authorize]
        public IEnumerable<ApiClient> GetApiKeys(string userName)
        {
            var result = from x in this._db.ApiClient
                         where x.UserName == userName
                         orderby x.IsActive descending, x.CreationDate descending
                         select x;
            return result.ToList();
        }

        [Authorize]
        public ApiThrottlePolicy GetApiThrottlePolicy(int throttlePolicyID)
        {
            var result = from policy in _db.ApiThrottlePolicy
                         where policy.ID == throttlePolicyID
                         select policy;

            return result.FirstOrDefault();
        }

        [Authorize]
        public ApiPermissionPolicy GetApiPermissionPolicy(int permissionPolicyID)
        {
            var result = from policy in _db.ApiPermissionPolicy
                         where policy.ID == permissionPolicyID
                         select policy;

            return result.FirstOrDefault();
        }

        [Authorize]
        public List<KeyValuePair<string, string>> GetApiClientKeyThrottlePolicies()
        {
            List<KeyValuePair<string, string>> policies = new List<KeyValuePair<string, string>>();

            var result = from client in this._db.ApiClient
                         join policy in _db.ApiThrottlePolicy on client.ApiThrottlePolicyID equals policy.ID
                         where client.IsActive == true
                         select new { client.PublicKey, policy.Policy };

            foreach (var policy in result)
            {
                policies.Add(new KeyValuePair<string, string>(policy.PublicKey, policy.Policy));
            }

            return policies;
        }

        public async Task<ApiClient> EditApiKey(string apiKey, string name, string description, string url, string redirectUrl)
        {
            DemandAuthentication();

            //Only allow users to edit ApiKeys if they IsActive == 1 and Current User owns it.
            var apiClient = (from x in _db.ApiClient
                             where x.PublicKey == apiKey && x.UserName == User.Identity.Name && x.IsActive == true
                             select x).FirstOrDefault();

            if (apiClient != null)
            {
                apiClient.AppAboutUrl = url;
                apiClient.RedirectUrl = redirectUrl;
                apiClient.AppDescription = description;
                apiClient.AppName = name;
                await _db.SaveChangesAsync().ConfigureAwait(CONSTANTS.AWAIT_CAPTURE_CONTEXT);
            }

            return apiClient;

        }

        [Authorize]
        public void CreateApiKey(string name, string description, string url, string redirectUrl)
        {
            DemandAuthentication();

            int? _defaultApiPermissionPolicyID = 1; // 1 is unrestricted, null is default

            ApiClient c = new ApiClient();
            c.IsActive = true;
            c.AppAboutUrl = url;
            c.RedirectUrl = redirectUrl;
            c.AppDescription = description;
            c.AppName = name;
            c.UserName = User.Identity.Name;
            c.CreationDate = CurrentDate;

            var generatePublicKey = new Func<string>(() =>
            {
                return String.Format("VO{0}AT", Guid.NewGuid().ToString().Replace("-", "").ToUpper());
            });

            //just make sure key isn't already in db
            var publicKey = generatePublicKey();
            while (_db.ApiClient.Any(x => x.PublicKey == publicKey))
            {
                publicKey = generatePublicKey();
            }

            c.PublicKey = publicKey;
            c.PrivateKey = (Guid.NewGuid().ToString() + Guid.NewGuid().ToString()).Replace("-", "").ToUpper();
            c.ApiPermissionPolicyID = _defaultApiPermissionPolicyID;

            //Using OAuth 2, we don't need enc keys
            //var keyGen = RandomNumberGenerator.Create();
            //byte[] tempKey = new byte[16];
            //keyGen.GetBytes(tempKey);
            //c.PublicKey = Convert.ToBase64String(tempKey);

            //tempKey = new byte[64];
            //keyGen.GetBytes(tempKey);
            //c.PrivateKey = Convert.ToBase64String(tempKey);

            _db.ApiClient.Add(c);
            _db.SaveChanges();
        }

        [Authorize]
        public ApiClient DeleteApiKey(int id)
        {
            DemandAuthentication();

            //Only allow users to delete ApiKeys if they IsActive == 1
            var apiKey = (from x in _db.ApiClient
                          where x.ID == id && x.UserName == User.Identity.Name && x.IsActive == true
                          select x).FirstOrDefault();

            if (apiKey != null)
            {
                apiKey.IsActive = false;
                _db.SaveChanges();
            }
            return apiKey;
        }

        public IEnumerable<ApiCorsPolicy> GetApiCorsPolicies()
        {
            var policy = (from x in _db.ApiCorsPolicy
                          where
                          x.IsActive
                          select x).ToList();
            return policy;
        }

        public ApiCorsPolicy GetApiCorsPolicy(string origin)
        {
            var domain = origin;

            //Match and pull domain only
            var domainMatch = Regex.Match(origin, @"://(?<domainPort>(?<domain>[\w.-]+)(?::\d+)?)[/]?");
            if (domainMatch.Success)
            {
                domain = domainMatch.Groups["domain"].Value;

                //var domain = domainMatch.Groups["domainPort"];
            }

            var policy = (from x in _db.ApiCorsPolicy

                              //haven't decided exactly how we are going to store origin (i.e. just the doamin name, with/without protocol, etc.)
                          where
                          (x.AllowOrigin.ToLower() == origin.ToLower()
                          || x.AllowOrigin.ToLower() == domain.ToLower())
                          && x.IsActive
                          select x).FirstOrDefault();
            return policy;
        }

        public void SaveApiLogEntry(ApiLog logentry)
        {
            logentry.CreationDate = CurrentDate;
            _db.ApiLog.Add(logentry);
            _db.SaveChanges();
        }

        public void UpdateApiClientLastAccessDate(int apiClientID)
        {
            var client = _db.ApiClient.Where(x => x.ID == apiClientID).FirstOrDefault();
            client.LastAccessDate = CurrentDate;
            _db.SaveChanges();
        }

    }
}
