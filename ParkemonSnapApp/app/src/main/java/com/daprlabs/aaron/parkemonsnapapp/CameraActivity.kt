package com.daprlabs.aaron.parkemonsnapapp

import android.content.Context
import android.content.Intent
import android.graphics.Bitmap
import android.hardware.Camera
import android.support.v7.app.AppCompatActivity
import android.os.Bundle
import android.provider.MediaStore
import android.util.AttributeSet
import android.view.SurfaceHolder
import android.view.SurfaceView
import kotlinx.android.synthetic.main.activity_camera.*

class CameraActivity : AppCompatActivity() {
//
//    var mCamera : android.hardware.Camera
//    var mPreview : CameraPreview? = null
//
//
//    init {
//        mCamera = Camera.open()
//    }

    val REQUEST_IMAGE_CAPTURE = 1

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_camera)

//        // Create an instance of Camera
//        // Create our Preview view and set it as the content of our activity.
//        mPreview = CameraPreview(this, mCamera)
//        framelayout.addView(mPreview)
        startActivityForResult(Intent(MediaStore.ACTION_IMAGE_CAPTURE), REQUEST_IMAGE_CAPTURE)
    }

    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        super.onActivityResult(requestCode, resultCode, data)

        if (requestCode === REQUEST_IMAGE_CAPTURE && resultCode === RESULT_OK) {
            val extras = data?.getExtras()
            val imageBitmap = extras?.get("data") as Bitmap

            //imageBitmap is the thumbnail of the image captured
        }
    }
}


class CameraPreview(context: Context?, camera : Camera) : SurfaceView(context), SurfaceHolder.Callback{

    val mCamera = camera
    val mHolder = holder

    init {
        mHolder.addCallback(this)
    }


    override fun surfaceChanged(holder: SurfaceHolder?, format: Int, width: Int, height: Int) {

    }

    override fun surfaceDestroyed(holder: SurfaceHolder?) {

    }

    override fun surfaceCreated(holder: SurfaceHolder?) {
        mCamera.setPreviewDisplay(mHolder)
        mCamera.startPreview()
    }


}