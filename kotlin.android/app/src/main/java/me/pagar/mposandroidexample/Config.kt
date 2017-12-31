package me.pagar.mposandroidexample;

import android.content.Context

var Context.currentDevice: String
    get() = getValue("currentDevice")
    set(value) = setValue("currentDevice", value)

private val sharedPreferencesKey = "Bifrost"

private fun Context.getValue(key: String): String {
	val sp = getSharedPreferences(sharedPreferencesKey, Context.MODE_PRIVATE)
	return sp.getString(key, "")
}

private fun Context.setValue(key: String, value: String) {
	val sp = getSharedPreferences(sharedPreferencesKey, Context.MODE_PRIVATE)
	val edit = sp.edit()

	edit.putString(key, value)
	edit.apply()
}
