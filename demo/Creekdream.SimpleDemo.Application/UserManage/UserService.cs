﻿using AutoMapper;
using Creekdream.Application.Service;
using Creekdream.Domain.Repositories;
using Creekdream.SimpleDemo.UserManage.Dto;
using Creekdream.Uow;
using System;
using System.Threading.Tasks;

namespace Creekdream.SimpleDemo.UserManage
{

    /// <inheritdoc />
    public class UserService : ApplicationService, IUserService
    {
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly IRepository<UserInfo, Guid> _userInfoRepository;
        private readonly IMapper _mapper;

        /// <inheritdoc />
        public UserService(
            IUnitOfWorkManager unitOfWorkManager,
            IRepository<User, Guid> userRepository,
            IRepository<UserInfo, Guid> userInfoRepository,
            IMapper mapper)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _userRepository = userRepository;
            _userInfoRepository = userInfoRepository;
            _mapper = mapper;
        }

        /// <inheritdoc />
        [UnitOfWork]
        public async Task<GetUserOutput> Create(CreateUserInput input)
        {
            var user = new User()
            {
                UserName = input.UserName,
                Password = input.Password
            };
            user = await _userRepository.InsertAsync(user);
            using (var uow = _unitOfWorkManager.Begin())
            {
                user.UserInfo = await AddUserInfo(user.Id, input.Name, input.Age);
                uow.Complete();
            }
            return _mapper.Map<GetUserOutput>(user);
        }

        /// <summary>
        /// 添加用户信息
        /// </summary>
        [UnitOfWork]
        public async Task<UserInfo> AddUserInfo(Guid userId, string name, int age)
        {
            var userInfo = new UserInfo()
            {
                Id = userId,
                Name = name,
                Age = age
            };
            return await _userInfoRepository.InsertAsync(userInfo);
        }
    }
}
