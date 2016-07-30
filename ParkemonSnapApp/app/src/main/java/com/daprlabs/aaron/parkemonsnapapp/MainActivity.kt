package com.daprlabs.aaron.parkemonsnapapp

import android.app.Activity
import android.content.Intent
import android.support.v7.app.AppCompatActivity
import android.os.Bundle
import kotlinx.android.synthetic.main.activity_upload_data.*

class MainActivity : AppCompatActivity() {

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        navigate<UploadDataActivity>("1")
    }


    inline fun <reified T : Activity> Activity.navigate(id: String) {
        val intent = Intent(this, T::class.java)
        intent.putExtra("id", id)
        startActivity(intent)
    }
}
