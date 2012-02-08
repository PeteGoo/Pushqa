using System;
using System.Activities;
using System.Reactive.Subjects;

namespace Sample.Web.Workflow {
    public class ProgressExtension {
        private Subject<int> Progress = new Subject<int>();
        public void SetProgress(int progress) {
            Progress.OnNext(progress);
        }

        
    }

    public class UpdateProgress : CodeActivity {

        public InArgument<int> Progress { get; set; }

        protected override void Execute(CodeActivityContext context) {
            ProgressExtension extension = context.GetExtension<ProgressExtension>();
            if(extension != null) {
                extension.SetProgress(Progress.Get(context));
            }
        }
    }
}