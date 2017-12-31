package me.pagar.mposandroidexample

import android.Manifest.permission.ACCESS_COARSE_LOCATION
import android.app.AlertDialog
import android.bluetooth.BluetoothAdapter
import android.bluetooth.BluetoothDevice
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.content.pm.PackageManager.PERMISSION_GRANTED
import android.os.Build
import android.os.Bundle
import android.support.annotation.RequiresApi
import android.support.v4.app.ActivityCompat
import android.support.v4.app.ActivityCompat.OnRequestPermissionsResultCallback
import android.support.v7.app.AppCompatActivity
import android.widget.Toast
import java.util.*


class DevicesActivity : AppCompatActivity(), OnRequestPermissionsResultCallback {

    private val abecsList = HashMap<String, BluetoothDevice>()
    private var dialog: AlertDialog? = null

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.empty)
        getBluetoothScanPermission()
    }

    override fun onPause() {
        super.onPause()

        if (registered) {
            unregisterReceiver(mReceiver)
            registered = false
        }
    }

    override fun onResume() {
        super.onResume()

        if (!registered) {
            getBluetoothScanPermission()

            if (dialog == null) {
                dialog = AlertDialog.Builder(this)
                        .setTitle(R.string.choose_device)
                        .setMessage(R.string.looking_for_device)
                        .setOnCancelListener { finish() }
                        .show()
            } else {
                dialog?.show()
            }
        }
    }

    private var requestCode = 1
    private var permission = ACCESS_COARSE_LOCATION
    private fun getBluetoothScanPermission() {
        ActivityCompat.requestPermissions(this, arrayOf(permission), requestCode)
    }

    override fun onRequestPermissionsResult(
            requestCode: Int, permissions: Array<String>, grantResults: IntArray
    ) {
        if (requestCode == this.requestCode) {
            for (p in permissions.indices) {
                val permission = permissions[p]
                if (permission == this.permission) {
                    if (grantResults[p] == PERMISSION_GRANTED) {
                        scanBluetooth()
                    } else {
                        Toast.makeText(
                            this, R.string.denyPermission, Toast.LENGTH_LONG
                        ).show()
                    }
                }
            }
        }
    }

    @RequiresApi(Build.VERSION_CODES.ECLAIR)
    private fun scanBluetooth() {
        val mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter()
        mBluetoothAdapter.startDiscovery()

        val filter = IntentFilter(BluetoothDevice.ACTION_FOUND)
        registerReceiver(mReceiver, filter)
        registered = true
    }

    private var registered = false
    private val mReceiver = object : BroadcastReceiver() {
        @RequiresApi(Build.VERSION_CODES.ECLAIR)
        override fun onReceive(context: Context, intent: Intent) {
            val action = intent.action
            if (BluetoothDevice.ACTION_FOUND == action) {

                val device = intent
                    .getParcelableExtra<BluetoothDevice>(BluetoothDevice.EXTRA_DEVICE)
                val deviceId = device.name + "\n" + device.address
                val isBonded = device.bondState == BluetoothDevice.BOND_BONDED

                if (isBonded && !abecsList.containsKey(deviceId)) {
                    abecsList.put(deviceId, device)
                    val mDeviceList = arrayOfNulls<CharSequence>(abecsList.size)

                    for(i in mDeviceList.indices) {
                        mDeviceList[i] = abecsList.keys.elementAt(i)
                    }

                    dialog?.dismiss()
                    dialog = AlertDialog.Builder(context)
                            .setTitle(R.string.choose_device)
                            .setItems(mDeviceList, { _, w -> run {
                                currentDevice = mDeviceList[w].toString()
                                finish()
                            }})
                            .setOnCancelListener { finish() }
                            .show()
                }
            }
        }
    }

}