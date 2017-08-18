/*
This file is the main entry point for defining Gulp tasks and using Gulp plugins.
Click here to learn more. https://go.microsoft.com/fwlink/?LinkId=518007
*/

var gulp = require('gulp');
var sass = require("gulp-sass");


gulp.task('default', function () {
    // place code for your default task here
    alert("hi");
});



var paths = {
    root: "./Content/",
    dest: "./wwwroot/css"
}

paths.scss = paths.root + "Sass/**/*.scss";
gulp.task('sass', function () {
    gulp.src(paths.scss)
        .pipe(sass())
        .pipe(gulp.dest(paths.dest));
});
