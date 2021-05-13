/// <binding AfterBuild='zipFiles' />
var gulp = require('gulp')
var git = require('gulp-git');
var zip = require('gulp-zip')
var fileName = 'UniversalPortalsMod'

function mergeChangesFromMaster(cb) {
    git.checkout('InSlimVML', function () {
        git.merge('master', function () {
            git.checkout('BepInEx', function () {
                git.merge('master', function () {
                    cb();
                })
            })
        })
    })
}

function zipFiles(cb) {
    gulp.src('bin/release/UniversalPortalsMod.dll')
        .pipe(zip(fileName + '.zip'))
        .pipe(gulp.dest('bin/release')).on('end', cb)
}

exports.mergeChangesFromMaster = mergeChangesFromMaster
exports.zipFiles = zipFiles