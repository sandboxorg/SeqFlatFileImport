using System.Runtime.CompilerServices;
using ApprovalTests;
using ApprovalTests.Reporters;
using NUnit.Framework;
using SeqFlatFileImport.Tests.Helpers;

namespace SeqFlatFileImport.Tests.Parse
{
    public class ParseTests
    {
        [Test]
        [UseReporter(typeof(BeyondCompareReporter))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void OctopusServer()
        {
            Execute("OctopusServer.txt");
        }

        [Test]
        [UseReporter(typeof(BeyondCompareReporter))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void OctopusWebLog()
        {
            Execute("Web-2016-05-29.log");
        }


        [Test]
        [UseReporter(typeof(BeyondCompareReporter))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Iis()
        {
            Execute("u_ex160412.log");
        }

        [Test]
        [UseReporter(typeof(BeyondCompareReporter))]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void OctopusTask()
        {
            Execute("ServerTasks-16572.log.txt");
        }


        public void Execute(string inputFileName)
        {
            var seqServer = new StubSeqEndpoint();
            var result = new Importer(seqServer)
                .Import(TestHelper.GetFilePath(inputFileName));
            result.ShouldBeSuccessful();
            Approvals.VerifyJson(seqServer.LogsAsJson);
        }


    }
}