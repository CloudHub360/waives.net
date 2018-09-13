# Contributing to Waives

:green_heart: *Thank you for taking the time to contribute!* :green_heart: We know that contributing
to an open-source project is time-consuming and sometimes can be stressful too. This guide is here to
help you with your contributions.

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [TL;DR](#tldr)
3. [How can I contribute?](#how-can-i-contribute)
   * [Reporting bugs](#reporting-bugs)
   * [Suggesting enhancements](#suggesting-enhancements)
   * [Your first contribution](#your-first-contribution)
   * [Pull requests](#pull-requests)
4. [Style guides](#style-guides)
5. [Additional notes](#additional-notes)

## Code of Conduct

This project and everyone participating in it is governed by the [Waives Code of Conduct](CODE_OF_CONDUCT.md).
By participating, you are expected to abide by and to uphold this code. Please report unacceptable behaviour
to [community@waives.io](mailto:community@waives.io).

## TL;DR

Please:

1. **Be friendly and respectful to all with whom you interact here.** Remember text is a lossy communication format
and what you write will most likely be interpreted differently from how it was meant.
2. **Use the bug and pull request templates provided.** These are here to make everyone's lives easier, contributors
and maintainers.
3. **Keep your commits and pull requests small.** A small commit is easier to understand, to cherry-pick, revert, and
undo. Pull requests are easier to review, and so will be merged more quickly, if you keep them small.
4. **Follow the coding guidelines.** We have tried to make these easy-to-follow and not too demanding; hopefully it
will be clear from the code files themselves, but there are also written guides for reference.
5. **Include clear and thoughtfully-worded tests.** Tests should only test one thing, and they should clearly describe
the behaviour they're trying to test. Take your time over test names.

:pray: Thank you for your time and contributions! :pray:

### But I just have a question to ask!

We'd love to answer your questions! We'll be happy to answer your questions via email at support@waives.io, and we
expect to implement lower-friction communication methods such as chat and message boards in time as the project and
community grows. Please do _not_ file issues to ask questions; these are for work items (e.g. bugs, features) only.

## How can I contribute?

### Reporting bugs

Reporting bugs is a fantastic way to contribute to the project. If you've hit an issue, we'd love to hear about it so we
can fix it and improve the project for all. Please check [this list](#before-submitting-a-bug-report) before submitting
your bug report; it may be you don't need to! Please [include as many details as possible](#how-do-i-submit-a-good-bug-report),
and use the [issue template](https://github.com/waives/waives.net/blob/master/.github/ISSUE_TEMPLATE/bug_report.md) to
guide your report; this will help us to resolve your issue more quickly!

> **Note:** if you find a **Closed** issue similar to the one you've encountered, please do **not** reopen that issue.
Instead, please create a new issue and include a link to the closed issue in your new one.

#### Before submitting a bug report

1. Please ensure you are using [the most recent version](https://github.com/waives/waives.net/releases/latest) of the SDK.

2. Please do [a quick search](https://github.com/search?q=+is%3Aissue+user%3Awaives) to see if the problem has already been reported.

   a. If there is a similar issue **and it is still open**, please add a comment to that issue with any new information you have.

   b. If there is a similar issue **and it is closed**, please do **not** reopen it. Instead, please create a new issue and include a link to the closed issue in your new one.

#### How do I submit a (good) bug report?

We track bugs as [GitHub Issues](https://guides.github.com/features/issues/). Create a new issue for your bug, and
select [the bug report template](https://github.com/waives/waives.net/blob/master/.github/ISSUE_TEMPLATE/bug_report.md).

Explain the problem you are experiencing, and include additional details to help the project maintainers reproduce the
problem:

* **Use a clear and descriptive title** for your issue which describes the problem. Don't be afraid of long titles.
* **Describe the exact steps to reproduce** providing as much detail as you can.
* **Clearly state which API you are using.** The Waives.NET SDK comprises two APIs, Pipelines and HTTP.
* **Provide specific examples to demonstrate the reproduction steps**. Include code snippets in [Markdown code blocks](https://help.github.com/articles/markdown-basics/#multiple-lines) to illustrate the issue, along with debugger data, console log output, etc. A minimal project reproducing the issue is the most ideal form of example.
* **Explain what behaviour you expected and why.**
* **Include supplementary resources.** A screen capture of a debugging session (provided as an [animated GIF](https://www.cockos.com/licecap/)) or console logs illustrating the issue will help the maintainers track down the problem will more quickly.
* **If the problem is related to performance or memory consumption**, please include a CPU profile capture with your report. We can accept profiles created with Visual Studio's debugging tools or JetBrains dotTrace and dotMemory tools.

Additional context which will speed up resolution:

* **Did this start happening after an update** or has this always been the case?
* **Can you reproduce the issue in an older version of the SDK** if it did start happening recently?
* **Can you reliably reproduce the issue** or does it come and go?

Include information about your development and runtime environment:

* **Which .NET language are you using?** C#, F#, VB, etc.
* **Are you targeting .NET Framework or .NET Core?**
* **Are you using .NET Framework projects or .NET Core SDK projects?**
* **Are you running on .NET Framework, Mono, or .NET Core?** Which version of the runtime are you using? Please include the patch version, i.e. say ".NET 4.7.0" rather than ".NET 4.7".
* **What is the name and version of the OS you're using?** If you're running Windows, please include the data output by the `winver` command.

### Suggesting enhancements

Enhancement suggestions are very welcome, from improving existing functionality to entirely new features. Following this
guide will help the maintainers and community understand your suggestion and find related suggestions.

Before submitting a suggestion, please check [this list](#before-submitting-an-enhancement-suggestion) as you may find
you don't need to create one. When creating an enhancement suggestion, please [include as many details as you can](#how-do-i-submit-a-good-enhancement-suggestion).
Fill in [the template](https://github.com/waives/waives.net/blob/master/.github/ISSUE_TEMPLATE/feature_request.md), and
include [code samples](https://help.github.com/articles/markdown-basics/#multiple-lines) illustrating how you would like
to consume your new feature if it existed.

#### Before submitting an enhancement suggestion

1. Indentify which API the enhancement belongs in: the HTTP API or the Pipelines API.
2. Please do [a quick search](https://github.com/search?q=+is%3Aissue+user%3Awaives) to see if the enhancement has already been suggested. If it has, **please add a comment to the existing issue** rather than opening a new one.

#### How do I submit a (good) enhancement suggestion?

Enhancement suggestions are tracked as [GitHub issues](https://guides.github.com/features/issues/). Create an issue
using [the provided template](https://github.com/waives/waives.net/blob/master/.github/ISSUE_TEMPLATE/feature_request.md)
and provide the following information:

* **Use a clear and descriptive title** for the issue to identify the suggestion.
* **Provide a step-by-step description of the suggested enhancement** in as many details as possible.
* **Provide specific examples to demonstrate the steps.** Include code samples (as [Markdown code blocks](https://help.github.com/articles/markdown-basics/#multiple-lines)) illustrating how you would like to consume the feature.
* **Explain why the enhancement would be useful** to most Waives.NET users and isn't something application-specific.
* **Specify the version of Waives.NET you're using**.
* **Specify the name and version of the OS you're using**. If you're running Windows, please include the data output by the `winver` command.

### Your first code contribution

If you're not sure where to begin contributing code to Waives.NET, take a look at the issues tagged `good first issue` and `help wanted`.

* [Good first issues](https://github.com/waives/waives.net/labels/good%20first%20issue) should generally only require a few lines of code and tests.
* [Help wanted](https://github.com/waives/waives.net/labels/help%20wanted) issues will be more involved than `good first issue`s. They may be an opportunity to work with other contributors.

### Pull requests

* **Fill in the required template for your change.** Unfortunately GitHub doesn't yet recognise the multiple pull request templates and offer a choice the way it does with issues, but you can load the template into the Pull Request form by appending `template=bug_report.md` or `template=feature_request.md` to the pull request query string.
* **Do not include issue numbers in the title.** Instead please use a clear, descriptive title and reference the issue number in the description body of the pull request.
* **Describe the work that you have completed in as much detail as possible.** Include [code samples](https://help.github.com/articles/markdown-basics/#multiple-lines) to illustrate any API changes you have introduced.
* **Include thoughtfully-worded, well-structured tests** in the `test/` folder. Run them using the build script as follows:

   ```powershell
   build/_init.ps1
   build test
   ```

* **Document all new public members you introduce.** Include at least `<summary />`, `<param />` and `<returns />` sections.
* **Avoid platform-dependent code.** Waives.NET targets .NET Standard 2.0 for the widest possible compatibility with .NET Framework, Mono, and .NET Core.
* Ensure `using`s are sorted alphabetically, with `System` namespaces at the top of the list. Ensure any unused `using`s are removed.
* Ensure `static` members are placed before instance members; fields before constructors before methods before properties; `public` before `internal` before `protected` before `private`.

## Style guides

## Additional notes
