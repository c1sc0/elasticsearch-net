﻿using FluentAssertions;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Framework;
using Tests.Framework.Integration;
using Tests.Framework.MockData;
using Xunit;

namespace Tests.Ingest
{
	[Collection(TypeOfCluster.Indexing)]
	public class PipelineCrudTests
		: CrudTestBase<IPutPipelineResponse, IGetPipelineResponse, IPutPipelineResponse, IDeletePipelineResponse>
	{
		public PipelineCrudTests(IndexingCluster cluster, EndpointUsage usage) : base(cluster, usage) { }

		protected override LazyResponses Create() => Calls<PutPipelineDescriptor, PutPipelineRequest, IPutPipelineRequest, IPutPipelineResponse>(
			CreateInitializer,
			CreateFluent,
			fluent: (s, c, f) => c.PutPipeline(s, f),
			fluentAsync: (s, c, f) => c.PutPipelineAsync(s, f),
			request: (s, c, r) => c.PutPipeline(r),
			requestAsync: (s, c, r) => c.PutPipelineAsync(r)
		);

		protected override void ExpectAfterCreate(IGetPipelineResponse response)
		{
			response.Pipelines.Should().NotBeNull().And.HaveCount(1);

			var pipeline = response.Pipelines.First();
			pipeline.Config.Should().NotBeNull();
			pipeline.Id.Should().NotBeNullOrEmpty();

			var processors = pipeline.Config.Processors;
			processors.Should().NotBeNull().And.HaveCount(2);

			var uppercase = processors.Where(p => p.Name == "uppercase").FirstOrDefault() as UppercaseProcessor;
			uppercase.Should().NotBeNull();
			uppercase.Field.Should().NotBeNull();

			var set = processors.Where(p => p.Name == "set").FirstOrDefault() as SetProcessor;
			set.Should().NotBeNull();
			set.Field.Should().NotBeNull();
			set.Value.Should().NotBeNull();
		}

		protected PutPipelineRequest CreateInitializer(string pipelineId) => new PutPipelineRequest(pipelineId)
		{
			Description = "Project Pipeline",
			Processors = new IProcessor[]
			{
				new UppercaseProcessor
				{
					Field = Infer.Field<Project>(p => p.State)
				},
				new SetProcessor
				{
					Field = Infer.Field<Project>(p => p.NumberOfCommits),
					Value = 0
				}
			}
		};

		protected IPutPipelineRequest CreateFluent(string pipelineId, PutPipelineDescriptor d) => d
			.Description("Project Pipeline")
			.Processors(ps => ps
				.Uppercase<Project>(u => u
					.Field(p => p.State)
				)
				.Set<Project>(s => s
					.Field(p => p.NumberOfCommits)
					.Value(0)
				)
			);

		protected override LazyResponses Read() => Calls<GetPipelineDescriptor, GetPipelineRequest, IGetPipelineRequest, IGetPipelineResponse>(
			GetInitializer,
			GetFluent,
			fluent: (s, c, f) => c.GetPipeline(s, f),
			fluentAsync: (s, c, f) => c.GetPipelineAsync(s, f),
			request: (s, c, r) => c.GetPipeline(r),
			requestAsync: (s, c, r) => c.GetPipelineAsync(r)
		);

		protected GetPipelineRequest GetInitializer(string pipelineId) => new GetPipelineRequest(pipelineId);

		protected IGetPipelineRequest GetFluent(string pipelineId, GetPipelineDescriptor d) => d;

		protected override LazyResponses Update() => Calls<PutPipelineDescriptor, PutPipelineRequest, IPutPipelineRequest, IPutPipelineResponse>(
			UpdateInitializer,
			UpdateFluent,
			fluent: (s, c, f) => c.PutPipeline(s, f),
			fluentAsync: (s, c, f) => c.PutPipelineAsync(s, f),
			request: (s, c, r) => c.PutPipeline(r),
			requestAsync: (s, c, r) => c.PutPipelineAsync(r)
		);

		protected PutPipelineRequest UpdateInitializer(string pipelineId) => new PutPipelineRequest(pipelineId)
		{
			Description = "Project Pipeline (updated)",
			Processors = new IProcessor[]
			{
				new UppercaseProcessor
				{
					Field = Infer.Field<Project>(p => p.State)
				},
				new SetProcessor
				{
					Field = Infer.Field<Project>(p => p.NumberOfCommits),
					Value = 500
				},
				new RenameProcessor
				{
					Field = Infer.Field<Project>(p => p.LeadDeveloper),
					TargetField = "techLead"
				}
			}
		};

		protected IPutPipelineRequest UpdateFluent(string pipelineId, PutPipelineDescriptor d) => d
			.Description("Project Pipeline (updated)")
			.Processors(ps => ps
				.Uppercase<Project>(u => u
					.Field(p => p.State)
				)
				.Set<Project>(s => s
					.Field(p => p.NumberOfCommits)
					.Value(500)
				)
				.Rename<Project>(s => s
					.Field(p => p.LeadDeveloper)
					.TargetField("techLead")
				)
			);

		protected override void ExpectAfterUpdate(IGetPipelineResponse response)
		{
			response.Pipelines.Should().NotBeNull().And.HaveCount(1);

			var pipeline = response.Pipelines.First();
			pipeline.Config.Should().NotBeNull();
			pipeline.Id.Should().NotBeNullOrEmpty();

			var processors = pipeline.Config.Processors;
			processors.Should().NotBeNull().And.HaveCount(3);

			var uppercase = processors.Where(p => p.Name == "uppercase").FirstOrDefault() as UppercaseProcessor;
			uppercase.Should().NotBeNull();
			uppercase.Field.Should().NotBeNull();

			var set = processors.Where(p => p.Name == "set").FirstOrDefault() as SetProcessor;
			set.Should().NotBeNull();
			set.Field.Should().NotBeNull();
			set.Value.Should().NotBeNull();

			var rename = processors.Where(p => p.Name == "rename").FirstOrDefault() as RenameProcessor;
			rename.Should().NotBeNull();
			rename.Field.Should().NotBeNull();
			rename.TargetField.Should().NotBeNull();
		}

		protected override LazyResponses Delete() => Calls<DeletePipelineDescriptor, DeletePipelineRequest, IDeletePipelineRequest, IDeletePipelineResponse>(
			DeleteInitializer,
			DeleteFluent,
			fluent: (s, c, f) => c.DeletePipeline(s, f),
			fluentAsync: (s, c, f) => c.DeletePipelineAsync(s, f),
			request: (s, c, r) => c.DeletePipeline(r),
			requestAsync: (s, c, r) => c.DeletePipelineAsync(r)
		);

		protected DeletePipelineRequest DeleteInitializer(string pipelineId) => new DeletePipelineRequest(pipelineId);

		protected IDeletePipelineRequest DeleteFluent(string pipelineId, DeletePipelineDescriptor d) => d;
	}
}
