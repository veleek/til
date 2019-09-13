# Testing `npm` packages locally

When working on a nodejs project you'll no doubt find yourself using a huge number of different [npm packages](https://npmjs.com).  Occasionally you may run into a bug and since the source is so easily accessible with these packages, it's super easy to make a tweak to fix your problem.  To help others take advantage of your work it's helpful to commit the changes back into the package repository, but to do so you will often want to test packages locally.  

## Temporarily linking to a local package

So, you pull down the source for the project, make the change, and now you need to test it out.  Usually, npm installs all of it's packages from the global repository into a `node_modules` folder inside your project.  You could just manually copy your source into that folder, but the next time you run `npm install` it will flatten any changes that you make.  Thankfully, you can use the [`npm link`](https://docs.npmjs.com/cli/link) command.  

```bash
cd /my/package/source

# Create a link for the development version of your package in the system repository
npm link

cd /my/project/

# Create a link in the local projects node_modules pointing to the development package
npm link <package-name>
```

Under the covers this is just a sym-link to the development folder that will play nice with npm.  If you run `npm install` it will remove the sym link and you'll be pointing back the the original package and you can run `npm link <package-name>` again in order re-create it.

## Permanently linking to a local package

After you've validated everything works you can commit you code and open a pull request to get it pulled into the official package.  But in the meantime you may still need to continue working on your project.  If you're working with others, or you're using some sort of cloud based build system, you'll need some way to let `npm install` use your development package.  Thankfully node's `package.json` supports referencing a local path, and as long as it's named correctly this package will be resolved as the correct dependency for any other packages.

In my case, there was [a bug in `clean-css`](https://github.com/jakubpawlowicz/clean-css/issues/1078), which was being indirectly referenced by `gulp-clean-css`.  After fixing the issue, I created a `node_modules_local` folder in my project directory and copied the fixed `clean-css` source into it and added it to git.  Then I updated `package.json` to reference this path directly:

```json
{
    ...
    "devDependencies": {
        ...
        "gulp-clean-css": "^3.10.0",
        "clean-css": "file:node_modules_local/clean-css",
        ...
    }
    ...
```

Now my Azure DevOps builds use the correct version of clean-css, and as soon as the change is merged you can remove the explicit dependency. 
 