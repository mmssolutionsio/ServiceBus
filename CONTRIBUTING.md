# How to contribute

We love getting patches to our ServiceBus library from the community. Here is a few guidelines that we
need contributors to follow so that we can have a chance of keeping on
top of things.

## Getting Started

* Make sure you have a [GitHub account](https://github.com/signup/free)
* [Create a new issue](https://github.com/MultimediaSolutionsAg/ServiceBus/issues/new), assuming one does not already exist.
  * Clearly describe the issue including steps to reproduce when it is a bug.
  * If it is a bug make sure you tell us what version you have encountered this bug on.
* Fork the repository on GitHub

## Making Changes

* Create a feature branch from where you want to base your work.
  * To quickly create a feature branch based on master; `git branch
    fix/master/my_contribution` then checkout the new branch with `git
    checkout fix/master/my_contribution`.  Please avoid working directly on the
    `master` branch.
* Make commits of logical units.
* Check for unnecessary whitespace with `git diff --check` before committing.
* Make sure your commit messages are in the proper format.
* Make sure you have added the necessary tests for your changes.
* Make sure your changes comply to our StyleCop rules
* Run a release build to assure nothing else was accidentally broken.

## Submitting Changes

* Push your changes to a feature branch in your fork of the repository.
* Submit a pull request to the ServiceBus repository

# Additional Resources

* [General GitHub documentation](http://help.github.com/)
* [GitHub pull request documentation](http://help.github.com/send-pull-requests/)
