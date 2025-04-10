# Automatically generated by scripts/boost/generate-ports.ps1

vcpkg_from_github(
    OUT_SOURCE_PATH SOURCE_PATH
    REPO boostorg/parameter_python
    REF boost-${VERSION}
    SHA512 64d32f11042db4d88f4c96e1f68c12cc9eee007398054ddd02c4b884247e2aa673ec69a3c51a7774e88d58c40758af63e2e523f46f0680bfd7ce3d5398cc3bb1
    HEAD_REF master
)

set(FEATURE_OPTIONS "")
boost_configure_and_install(
    SOURCE_PATH "${SOURCE_PATH}"
    OPTIONS ${FEATURE_OPTIONS}
)
