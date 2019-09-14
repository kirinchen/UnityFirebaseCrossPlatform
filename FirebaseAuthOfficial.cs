using UnityEngine;
using System.Collections;
using Firebase.Auth;

namespace surfm.tool.realtimedb {
    public class FirebaseAuthOfficial : FirebaseAuther {
        private Firebase.Auth.FirebaseUser user;
        private CallbackList initCB = new CallbackList();
        private string email = ConstantRepo.getInstance().get<string>("RealtimeDB.email");
        private string pass = ConstantRepo.getInstance().get<string>("RealtimeDB.pass");

        public FirebaseAuther auth() {
            if (initCB.isDone()) return this;
            FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            auth.SignInWithEmailAndPasswordAsync(email, pass).ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                user = task.Result;
                UnityMainThreadDispatcher.uniRxRun(() => {
                    initCB.done();
                    Debug.Log("auth.done");
                });
                Debug.LogFormat("Firebase user login successfully: {0} ({1})",
                    user.DisplayName, user.UserId);


            });
            return this;
        }

        public CallbackList authDoneCB() {
            return initCB;
        }
    }
}
