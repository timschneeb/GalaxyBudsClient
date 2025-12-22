#!/bin/sh
if [[ $1 = "aab" ]] || [[ $1 = "apk" ]] && [[ -n $2 ]]; then
  :
else
  echo "Usage: "$0" [aab|apk] [additional_prop]"
  exit 1
fi

if [[ -z $PASS ]]; then
  echo "Env var PASS to decrypt keystore not set"
  exit 1
fi

# Workaround: set IsAndroid=true globally, the property doesn't propegate to subprojects properly for some reason
dotnet publish -f net10.0-android36.1 -c Debug -p:AndroidKeyStore=true -p:AndroidSigningKeyStore=/home/tim/Development/_KeyStores/keystore.jks -p:AndroidPackageFormats=$1 -p:AndroidSigningKeyAlias=galaxybudsclient -p:AndroidSdkDirectory=/home/tim/Android/Sdk -p:AndroidSigningKeyPass="env:PASS" -p:AndroidSigningStorePass="env:PASS" -p:DebugType=PdbOnly -p:NotDebuggable=true -p:EmbedAssembliesIntoApk=true -p:IsAndroid=true -p:$2
