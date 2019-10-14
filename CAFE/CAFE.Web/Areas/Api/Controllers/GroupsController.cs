
using System.Collections.Generic;
using System.Web.Http;
using AutoMapper;
using CAFE.Core.Security;
using CAFE.Web.Areas.Api.Models;
using CAFE.Core.Misc;

namespace CAFE.Web.Areas.Api.Controllers
{
    /// <summary>
    /// API endpoint for manage users groups
    /// </summary>
    [Authorize(Roles = Constants.AdminRoleName)]
    public class GroupsController : ApiController
    {
        private readonly ISecurityService _securityService;

        public GroupsController(ISecurityService securityService)
        {
            _securityService = securityService;
        }


        /// <summary>
        /// Returns all groups
        /// </summary>
        /// <returns>Groups collection</returns>
        [HttpGet]
        public GroupViewModel[] GetGroupList()
        {
            var groups = _securityService.GetAllGroups();
            var groupModels = Mapper.Map<IEnumerable<Group>, List<GroupViewModel>>(groups);

            return groupModels.ToArray();
        }

        /// <summary>
        /// Adds new group
        /// </summary>
        /// <param name="group">Group data</param>
        /// <returns>Newly added group</returns>
        [HttpPost]
        public GroupViewModel AddGroup(GroupViewModel group)
        {
           var groupDbModel = Mapper.Map<GroupViewModel, Group>(group);
           groupDbModel.Id = System.Guid.NewGuid().ToString();

           var newDbGroup = _securityService.AddGroup(groupDbModel);

           var newDbGroupView = Mapper.Map<Group, GroupViewModel>(newDbGroup);

            return newDbGroupView;
        }

        /// <summary>
        /// Removes a group
        /// </summary>
        /// <param name="group">Group to remove</param>
        /// <returns>Status of operation true/false</returns>
        [HttpPost]
        public bool DeleteGroup(Group group)
        {
            return _securityService.RemoveGroup(group);
        }

        /// <summary>
        /// Search users for group
        /// </summary>
        /// <param name="userModel">User search data</param>
        /// <returns>Found users</returns>
        [HttpPost]
        public IEnumerable<UserViewModel> SearchUsers(User userModel)
        {
            var key = userModel.Name;
            var foundDbUSers = _securityService.SearchUsers(key);

            var usersCollectionView = Mapper.Map<IEnumerable<User>, List<UserViewModel>>(foundDbUSers);

            return usersCollectionView;
        }

        /// <summary>
        /// Updates group data
        /// </summary>
        /// <param name="userAndGroupId">User and Group ID</param>
        /// <returns>Status of operation true/false</returns>
        [HttpPost]
        public bool UpdateGroup(AddUserToGroupModel userAndGroupId)
        {
            return _securityService.UpdateGroup(
                groupId: userAndGroupId.groupId,
                groupName: userAndGroupId.groupName,
                userAddedIds: userAndGroupId.userAddedIds,
                userDeletedIds: userAndGroupId.userDeletedIds
            );
        }
        
    }
}