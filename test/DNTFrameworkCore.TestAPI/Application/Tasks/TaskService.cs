using System;
using System.Linq;
using AutoMapper;
using DNTFrameworkCore.Application.Services;
using DNTFrameworkCore.Cryptography;
using DNTFrameworkCore.EFCore.Application;
using DNTFrameworkCore.EFCore.Context;
using DNTFrameworkCore.EFCore.Context.Extensions;
using DNTFrameworkCore.Eventing;
using DNTFrameworkCore.Linq;
using DNTFrameworkCore.TestAPI.Application.Tasks.Models;
using Microsoft.EntityFrameworkCore;
using Task = DNTFrameworkCore.TestAPI.Domain.Tasks.Task;

namespace DNTFrameworkCore.TestAPI.Application.Tasks
{
    public interface ITaskService : ICrudService<int, TaskReadModel, TaskModel, TaskFilteredPagedQueryModel>
    {
        bool TamperedTaskExists();
    }

    public class TaskService : CrudService<Task, int, TaskReadModel, TaskModel, TaskFilteredPagedQueryModel>,
        ITaskService
    {
        private readonly IMapper _mapper;

        public TaskService(
            IUnitOfWork uow,
            IEventBus bus,
            IMapper mapper) : base(uow, bus)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public bool TamperedTaskExists()
        {
            var tasks = EntitySet.ToList();
            return tasks.Any(task => task.Hash != UnitOfWork.EntityHash(task));
        }

        protected override IQueryable<TaskReadModel> BuildReadQuery(TaskFilteredPagedQueryModel model)
        {
            return EntitySet.AsNoTracking()
                .WhereIf(model.State.HasValue, t => t.State == model.State)
                .Select(t => new TaskReadModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    State = t.State,
                    Number = t.Number
                });
        }

        protected override void MapToEntity(TaskModel model, Task task)
        {
            _mapper.Map(model, task);
            task.BranchId = 3;
        }

        protected override TaskModel MapToModel(Task task)
        {
            return _mapper.Map<TaskModel>(task);
        }
    }
}