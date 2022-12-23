using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.NCProgramConcatenationForHoleDrilling.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Services.Dialogs;

namespace Wada.NCProgramConcatenationForHoleDrilling.ViewModels.Tests
{
    [TestClass()]
    public class NotationContentConfirmationDialogViewModelTests
    {
        [TestMethod()]
        public void 正常系_ダイアログの初期状態が正しいこと()
        {
            // given
            // when
            NotationContentConfirmationDialogViewModel viewModel = new();

            // then
            string expected = "注記内容確認";
            Assert.AreEqual(expected, viewModel.Title);
        }

        [TestMethod()]
        public void 正常系_OKボタンを押すとButtonResult_OKが返ってくること()
        {
            // given
            // when
            NotationContentConfirmationDialogViewModel viewModel = new();
            IDialogResult? result = default;
            viewModel.RequestClose += (dialogResult) => result = dialogResult;
            viewModel.ExecCommand.Execute();

            // then
            Assert.IsTrue(viewModel.ExecCommand.CanExecute());
            Assert.AreEqual(ButtonResult.OK, result?.Result);
        }

        [TestMethod()]
        public void 正常系_キャンセルボタンを押すとButtonResult_Cancelが返ってくること()
        {
            // given
            // when
            NotationContentConfirmationDialogViewModel viewModel = new();
            IDialogResult? dialogResult = default;
            viewModel.RequestClose += (result) => dialogResult = result;
            viewModel.CancelCommand.Execute();

            // then
            Assert.IsTrue(viewModel.CancelCommand.CanExecute());
            Assert.AreEqual(ButtonResult.Cancel, dialogResult?.Result);
        }
    }
}