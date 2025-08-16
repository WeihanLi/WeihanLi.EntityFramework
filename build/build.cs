var solutionPath = "./WeihanLi.EntityFramework.slnx";
string[] srcProjects = [ 
    "./src/WeihanLi.EntityFramework/WeihanLi.EntityFramework.csproj"
];
string[] testProjects = [ "./test/WeihanLi.EntityFramework.Test/WeihanLi.EntityFramework.Test.csproj" ];

await DotNetPackageBuildProcess
    .Create(options => 
    {
        options.SolutionPath = solutionPath;
        options.SrcProjects = srcProjects;
        options.TestProjects = testProjects;
    })
    .ExecuteAsync(args);
